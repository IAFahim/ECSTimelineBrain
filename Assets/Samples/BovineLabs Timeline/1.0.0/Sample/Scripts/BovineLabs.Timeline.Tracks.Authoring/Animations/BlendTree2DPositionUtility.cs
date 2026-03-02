using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BovineLabs.Timeline.Authoring
{
    public static class BlendTree2DPositionUtility
    {
        /// <summary>
        /// When true, all clips are forced to walk magnitude (0.5) regardless of
        /// "run"/"walk" keywords. Set from BlendTree2DTrack before calling any method.
        /// </summary>
        public static bool WalkOnly;

        // ── Longitudinal axis: 0° = forward (+Y), 180° = backward (−Y) ───────
        private static readonly (string[] Keywords, float Deg)[] LongitudinalMap =
        {
            (new[] { "forward", "front", "fwd", "north" },   0f),
            (new[] { "backward", "back",  "bwd", "south" }, 180f),
        };

        // ── Lateral axis: 90° = right (+X), 270° = left (−X) ─────────────────
        private static readonly (string[] Keywords, float Deg)[] LateralMap =
        {
            (new[] { "right", "east", "straferight" },  90f),
            (new[] { "left",  "west", "strafeleft"  }, 270f),
        };

        // ── Speed scalars ──────────────────────────────────────────────────────
        private static readonly (string[] Keywords, float Scale)[] SpeedMap =
        {
            (new[] { "walk",   "slow",   "stroll" }, 0.5f ),
            (new[] { "jog"                         }, 0.75f),
            (new[] { "sprint", "dash"              }, 1.0f ),
            (new[] { "run"                         }, 1.0f ),
        };

        // ── Exact overrides (checked before any decomposition) ─────────────────
        private static readonly (string[] Keywords, Vector2 Position)[] ExactOverrides =
        {
            (new[] { "idle", "stand", "neutral", "center", "zero", "tpose", "apose" }, Vector2.zero),
            (new[] { "crouch", "duck", "prone"                                       }, new Vector2(0f, -0.5f)),
        };

        // Matches 2–3 digit numbers not immediately adjacent to letters.
        // Avoids false positives like "v2", "clip01", "blend3D".
        private static readonly Regex AngleRegex =
            new Regex(@"(?<![a-z])(\d{2,3})(?!\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ──────────────────────────────────────────────────────────────────────
        //  Public API
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Infers a 2D blend-tree position from <paramref name="clipName"/>.
        /// Returns false when no directional keyword is found at all.
        /// </summary>
        public static bool TryInferPosition(string clipName, out Vector2 position)
        {
            position = Vector2.zero;
            if (string.IsNullOrEmpty(clipName)) return false;

            var n = Normalise(clipName);

            foreach (var (kws, pos) in ExactOverrides)
            foreach (var kw in kws)
                if (n.Contains(kw))
                {
                    position = pos;
                    return true;
                }

            float scale = WalkOnly ? 0.5f : DetectScale(n);

            float? longDeg = DetectAxis(n, LongitudinalMap);
            float? latDeg = DetectAxis(n, LateralMap);

            if (longDeg == null && latDeg == null) return false;

            if (latDeg == null)
            {
                position = CompassToVec(longDeg!.Value) * scale;
                return true;
            }
            if (longDeg == null)
            {
                position = CompassToVec(latDeg!.Value) * scale;
                return true;
            }

            var m = AngleRegex.Match(n);
            if (m.Success && float.TryParse(m.Value, out float angle) && angle >= 10f && angle <= 170f)
            {
                float sign = (Mathf.Approximately(latDeg.Value, 90f)) ? 1f : -1f;
                float finalDeg = (longDeg.Value + angle * sign + 360f) % 360f;
                position = CompassToVec(finalDeg) * scale;
            }
            else
            {
                float offset = (Mathf.Approximately(latDeg.Value, 90f)) ? 30f : -30f;
                float finalDeg = (longDeg.Value + offset + 360f) % 360f;
                position = CompassToVec(finalDeg) * scale;
            }
            return true;
        }
        
        
        public static Vector2[] DistributeOnCircle(int count, bool includeCenter = false, float radius = 1f)
        {
            if (count <= 0) return Array.Empty<Vector2>();
            var result    = new Vector2[count];
            int ringCount = includeCenter ? count - 1 : count;
            int offset    = includeCenter ? 1 : 0;
            if (includeCenter) result[0] = Vector2.zero;
            if (ringCount == 0) return result;
            float step = 360f / ringCount;
            for (int i = 0; i < ringCount; i++)
            {
                float rad      = Mathf.Deg2Rad * (90f + step * i);
                result[i + offset] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            }
            return result;
        }

        public static Vector2[] DistributeOnGrid(int count)
        {
            if (count <= 0) return Array.Empty<Vector2>();
            int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.CeilToInt((float)count / cols);
            var result = new Vector2[count];
            int idx = 0;
            for (int r = 0; r < rows && idx < count; r++)
            for (int c = 0; c < cols && idx < count; c++, idx++)
                result[idx] = new Vector2(
                    cols > 1 ? Mathf.Lerp(-1f,  1f, (float)c / (cols - 1)) : 0f,
                    rows > 1 ? Mathf.Lerp( 1f, -1f, (float)r / (rows - 1)) : 0f);
            return result;
        }

        public static void AutoDistribute(IList<BlendTree2DTrack.BlendTree2DMotionEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;
            var unmatched = new List<int>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                if (e.Clip != null && TryInferPosition(e.Clip.name, out var pos))
                    e.Position = pos;
                else
                    unmatched.Add(i);
            }
            if (unmatched.Count > 0)
            {
                var ring = DistributeOnCircle(unmatched.Count, false, 0.75f);
                for (int j = 0; j < unmatched.Count; j++)
                {
                    var e = entries[unmatched[j]];
                    if (e != null) e.Position = ring[j];
                }
            }
        }
        public static void DistributeUniform(IList<BlendTree2DTrack.BlendTree2DMotionEntry> entries,
                                             bool includeCenter = false)
        {
            if (entries == null || entries.Count == 0) return;
            var positions = DistributeOnCircle(entries.Count, includeCenter);
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e != null) e.Position = positions[i];
            }
        }

        private static Vector2 CompassToVec(float compassDeg)
        {
            float rad = Mathf.Deg2Rad * (90f - compassDeg);
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
        
        private static float ShortestArcDeg(float from, float to)
        {
            float diff = ((to - from + 180f) % 360f) - 180f;
            return diff;
        }

        private static float ShortestArcAvg(float a, float b)
        {
            float diff = ShortestArcDeg(a, b);
            return (a + diff * 0.5f + 360f) % 360f;
        }

        private static float? DetectAxis(string normalised, (string[] Keywords, float Deg)[] map)
        {
            foreach (var (kws, deg) in map)
                foreach (var kw in kws)
                    if (normalised.Contains(kw)) return deg;
            return null;
        }

        private static float DetectScale(string normalised)
        {
            foreach (var (kws, scale) in SpeedMap)
                foreach (var kw in kws)
                    if (normalised.Contains(kw)) return scale;
            return 1f;
        }

        private static string Normalise(string s) =>
            s.ToLowerInvariant()
             .Replace("_", "").Replace("-", "").Replace(" ", "").Replace("@", "");
    }
}