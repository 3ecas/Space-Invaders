using System;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// A tiny software renderer used to draw the game's placeholder art from code.
    /// Shapes are described in "units": y runs from -1 (bottom) to +1 (top) and x uses the same
    /// scale, so a square canvas spans -1..1 both ways. Everything is drawn at several times the
    /// final resolution and scaled down at the end, which gives smooth anti-aliased edges.
    /// </summary>
    public sealed class SpriteCanvas
    {
        public readonly int Width;
        public readonly int Height;

        readonly int supersample;
        readonly int w;
        readonly int h;
        readonly float scale;
        readonly float centerX;
        readonly float centerY;
        readonly Color[] pixels;

        public SpriteCanvas(int width, int height, int supersample = 4)
        {
            Width = width;
            Height = height;
            this.supersample = supersample;
            w = width * supersample;
            h = height * supersample;
            scale = h * 0.5f;
            centerX = w * 0.5f;
            centerY = h * 0.5f;
            pixels = new Color[w * h];
        }

        /// <summary>Half the canvas width in units (1 for a square canvas).</summary>
        public float HalfWidth => (float)Width / Height;

        // ---------------------------------------------------------------- core fill

        /// <summary>
        /// Paints every pixel in the box for which <paramref name="coverage"/> returns more than 0.
        /// The returned value (0..1) scales the colour's opacity, which allows soft gradients.
        /// </summary>
        public void Fill(Color color, float minX, float minY, float maxX, float maxY, Func<float, float, float> coverage)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt(centerX + minX * scale), 0, w - 1);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(centerX + maxX * scale), 0, w - 1);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(centerY + minY * scale), 0, h - 1);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(centerY + maxY * scale), 0, h - 1);
            float inverse = 1f / scale;

            for (int py = y0; py <= y1; py++)
            {
                float y = (py + 0.5f - centerY) * inverse;
                int row = py * w;
                for (int px = x0; px <= x1; px++)
                {
                    float x = (px + 0.5f - centerX) * inverse;
                    float amount = coverage(x, y);
                    if (amount <= 0f) continue;

                    Color source = color;
                    source.a *= Mathf.Clamp01(amount);
                    pixels[row + px] = Over(source, pixels[row + px]);
                }
            }
        }

        /// <summary>
        /// Like <see cref="Fill"/>, but the callback decides the colour of every pixel
        /// (return alpha 0 to leave a pixel untouched). Used for planets and other shaded surfaces.
        /// </summary>
        public void Paint(float minX, float minY, float maxX, float maxY, Func<float, float, Color> shader)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt(centerX + minX * scale), 0, w - 1);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(centerX + maxX * scale), 0, w - 1);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(centerY + minY * scale), 0, h - 1);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(centerY + maxY * scale), 0, h - 1);
            float inverse = 1f / scale;

            for (int py = y0; py <= y1; py++)
            {
                float y = (py + 0.5f - centerY) * inverse;
                int row = py * w;
                for (int px = x0; px <= x1; px++)
                {
                    float x = (px + 0.5f - centerX) * inverse;
                    Color source = shader(x, y);
                    if (source.a <= 0f) continue;
                    pixels[row + px] = Over(source, pixels[row + px]);
                }
            }
        }

        /// <summary>Standard "source over destination" alpha blending.</summary>
        static Color Over(Color top, Color bottom)
        {
            float alpha = top.a + bottom.a * (1f - top.a);
            if (alpha <= 0.0001f) return new Color(0f, 0f, 0f, 0f);

            float keep = bottom.a * (1f - top.a);
            return new Color(
                (top.r * top.a + bottom.r * keep) / alpha,
                (top.g * top.a + bottom.g * keep) / alpha,
                (top.b * top.a + bottom.b * keep) / alpha,
                alpha);
        }

        // ---------------------------------------------------------------- shapes

        /// <summary>A filled polygon. Points are listed as x0, y0, x1, y1, ...</summary>
        public void Polygon(Color color, params float[] points)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < points.Length; i += 2)
            {
                minX = Mathf.Min(minX, points[i]);
                maxX = Mathf.Max(maxX, points[i]);
                minY = Mathf.Min(minY, points[i + 1]);
                maxY = Mathf.Max(maxY, points[i + 1]);
            }
            Fill(color, minX, minY, maxX, maxY, (x, y) => InsidePolygon(points, x, y) ? 1f : 0f);
        }

        /// <summary>A polygon plus its mirror image across the vertical centre line - handy for ships.</summary>
        public void PolygonMirrored(Color color, params float[] points)
        {
            Polygon(color, points);
            var mirrored = new float[points.Length];
            for (int i = 0; i < points.Length; i += 2)
            {
                mirrored[i] = -points[i];
                mirrored[i + 1] = points[i + 1];
            }
            Polygon(color, mirrored);
        }

        static bool InsidePolygon(float[] p, float x, float y)
        {
            bool inside = false;
            int count = p.Length / 2;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                float xi = p[i * 2], yi = p[i * 2 + 1];
                float xj = p[j * 2], yj = p[j * 2 + 1];
                if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) inside = !inside;
            }
            return inside;
        }

        public void Ellipse(Color color, float cx, float cy, float rx, float ry)
        {
            Fill(color, cx - rx, cy - ry, cx + rx, cy + ry, (x, y) =>
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                return dx * dx + dy * dy <= 1f ? 1f : 0f;
            });
        }

        public void Circle(Color color, float cx, float cy, float radius) => Ellipse(color, cx, cy, radius, radius);

        public void CircleMirrored(Color color, float cx, float cy, float radius)
        {
            Circle(color, cx, cy, radius);
            Circle(color, -cx, cy, radius);
        }

        public void Ring(Color color, float cx, float cy, float outer, float inner)
        {
            Fill(color, cx - outer, cy - outer, cx + outer, cy + outer, (x, y) =>
            {
                float d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                return d2 <= outer * outer && d2 >= inner * inner ? 1f : 0f;
            });
        }

        public void Rect(Color color, float x0, float y0, float x1, float y1)
        {
            Fill(color, x0, y0, x1, y1, (x, y) => x >= x0 && x <= x1 && y >= y0 && y <= y1 ? 1f : 0f);
        }

        public void RectMirrored(Color color, float x0, float y0, float x1, float y1)
        {
            Rect(color, x0, y0, x1, y1);
            Rect(color, -x1, y0, -x0, y1);
        }

        public void RoundedRect(Color color, float x0, float y0, float x1, float y1, float radius)
        {
            Fill(color, x0, y0, x1, y1, (x, y) =>
            {
                // Distance to the rectangle shrunk by the corner radius.
                float dx = Mathf.Max(Mathf.Max(x0 + radius - x, x - (x1 - radius)), 0f);
                float dy = Mathf.Max(Mathf.Max(y0 + radius - y, y - (y1 - radius)), 0f);
                return dx * dx + dy * dy <= radius * radius ? 1f : 0f;
            });
        }

        /// <summary>A thick line with rounded ends.</summary>
        public void Capsule(Color color, float ax, float ay, float bx, float by, float radius)
        {
            float minX = Mathf.Min(ax, bx) - radius, maxX = Mathf.Max(ax, bx) + radius;
            float minY = Mathf.Min(ay, by) - radius, maxY = Mathf.Max(ay, by) + radius;
            float abx = bx - ax, aby = by - ay;
            float lengthSqr = abx * abx + aby * aby;

            Fill(color, minX, minY, maxX, maxY, (x, y) =>
            {
                float t = lengthSqr > 0f ? Mathf.Clamp01(((x - ax) * abx + (y - ay) * aby) / lengthSqr) : 0f;
                float dx = x - (ax + abx * t);
                float dy = y - (ay + aby * t);
                return dx * dx + dy * dy <= radius * radius ? 1f : 0f;
            });
        }

        public void CapsuleMirrored(Color color, float ax, float ay, float bx, float by, float radius)
        {
            Capsule(color, ax, ay, bx, by, radius);
            Capsule(color, -ax, ay, -bx, by, radius);
        }

        /// <summary>A soft blob: full strength in the middle, fading to nothing at the radius.</summary>
        public void Glow(Color color, float cx, float cy, float radius, float falloff = 2f)
        {
            Fill(color, cx - radius, cy - radius, cx + radius, cy + radius, (x, y) =>
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / radius;
                return d >= 1f ? 0f : Mathf.Pow(1f - d, falloff);
            });
        }

        // ---------------------------------------------------------------- shading passes

        /// <summary>Darkens everything right of the centre line: a cheap way to make a ship look three-dimensional.</summary>
        public void ShadeRightHalf(float amount)
        {
            float keep = 1f - amount;
            int start = Mathf.CeilToInt(centerX);
            for (int py = 0; py < h; py++)
            {
                int row = py * w;
                for (int px = start; px < w; px++)
                {
                    Color c = pixels[row + px];
                    if (c.a <= 0f) continue;
                    pixels[row + px] = new Color(c.r * keep, c.g * keep, c.b * keep, c.a);
                }
            }
        }

        /// <summary>Brightens the top and darkens the bottom of whatever has been drawn so far.</summary>
        public void ShadeVertical(float topMultiplier, float bottomMultiplier)
        {
            for (int py = 0; py < h; py++)
            {
                float m = Mathf.Lerp(bottomMultiplier, topMultiplier, py / (float)(h - 1));
                int row = py * w;
                for (int px = 0; px < w; px++)
                {
                    Color c = pixels[row + px];
                    if (c.a <= 0f) continue;
                    pixels[row + px] = new Color(Mathf.Clamp01(c.r * m), Mathf.Clamp01(c.g * m), Mathf.Clamp01(c.b * m), c.a);
                }
            }
        }

        /// <summary>Lights existing pixels as if they were a ball lit from (lightX, lightY).</summary>
        public void ShadeSphere(float cx, float cy, float radius, float lightX, float lightY, float lit, float shadow)
        {
            float inverse = 1f / scale;
            for (int py = 0; py < h; py++)
            {
                float y = (py + 0.5f - centerY) * inverse;
                int row = py * w;
                for (int px = 0; px < w; px++)
                {
                    Color c = pixels[row + px];
                    if (c.a <= 0f) continue;

                    float x = (px + 0.5f - centerX) * inverse;
                    float d = Mathf.Sqrt((x - lightX) * (x - lightX) + (y - lightY) * (y - lightY)) / (radius * 1.6f);
                    float m = Mathf.Lerp(lit, shadow, Mathf.Clamp01(d));
                    pixels[row + px] = new Color(Mathf.Clamp01(c.r * m), Mathf.Clamp01(c.g * m), Mathf.Clamp01(c.b * m), c.a);
                }
            }
        }

        /// <summary>Adds an outline of the given thickness (in final pixels) behind everything drawn so far.</summary>
        public void Outline(Color color, float thicknessPixels)
        {
            int radius = Mathf.Max(1, Mathf.RoundToInt(thicknessPixels * supersample));
            int radiusSqr = radius * radius;

            var alpha = new float[pixels.Length];
            for (int i = 0; i < pixels.Length; i++) alpha[i] = pixels[i].a;

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    int index = py * w + px;
                    if (alpha[index] >= 0.999f) continue;

                    float best = 0f;
                    int yMin = Mathf.Max(0, py - radius), yMax = Mathf.Min(h - 1, py + radius);
                    int xMin = Mathf.Max(0, px - radius), xMax = Mathf.Min(w - 1, px + radius);

                    for (int ny = yMin; ny <= yMax && best < 0.999f; ny++)
                    {
                        int dy = ny - py;
                        int row = ny * w;
                        for (int nx = xMin; nx <= xMax; nx++)
                        {
                            int dx = nx - px;
                            if (dx * dx + dy * dy > radiusSqr) continue;
                            float a = alpha[row + nx];
                            if (a > best)
                            {
                                best = a;
                                if (best >= 0.999f) break;
                            }
                        }
                    }

                    if (best <= 0f) continue;
                    Color under = color;
                    under.a *= best;
                    pixels[index] = Over(pixels[index], under);
                }
            }
        }

        /// <summary>Turns the drawing upside down. Enemy ships are designed nose-up, then flipped to face the player.</summary>
        public void FlipVertical()
        {
            for (int py = 0; py < h / 2; py++)
            {
                int top = (h - 1 - py) * w;
                int bottom = py * w;
                for (int px = 0; px < w; px++)
                {
                    Color swap = pixels[bottom + px];
                    pixels[bottom + px] = pixels[top + px];
                    pixels[top + px] = swap;
                }
            }
        }

        // ---------------------------------------------------------------- output

        /// <summary>Scales the drawing down to its final size.</summary>
        public Texture2D ToTexture(bool whiteSilhouette = false)
        {
            var output = new Color[Width * Height];
            float samples = supersample * supersample;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float r = 0f, g = 0f, b = 0f, a = 0f;
                    for (int sy = 0; sy < supersample; sy++)
                    {
                        int row = (y * supersample + sy) * w + x * supersample;
                        for (int sx = 0; sx < supersample; sx++)
                        {
                            Color c = pixels[row + sx];
                            r += c.r * c.a;
                            g += c.g * c.a;
                            b += c.b * c.a;
                            a += c.a;
                        }
                    }

                    if (a <= 0f) output[y * Width + x] = new Color(0f, 0f, 0f, 0f);
                    else if (whiteSilhouette) output[y * Width + x] = new Color(1f, 1f, 1f, a / samples);
                    else output[y * Width + x] = new Color(r / a, g / a, b / a, a / samples);
                }
            }

            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            texture.SetPixels(output);
            texture.Apply();
            return texture;
        }
    }
}
