using System.Collections.Generic;
using NUnit.Framework;
using static Csg.Solids;

namespace Csg.Test
{
	[TestFixture]
	public class PipeChainUnionTest
	{
		static int TriangleCount(Solid solid)
		{
			var count = 0;
			foreach (var polygon in solid.Polygons)
			{
				if (polygon.Vertices.Count >= 3)
				{
					count += polygon.Vertices.Count - 2;
				}
			}
			return count;
		}

		static List<Solid> BuildPipeChain(int segmentCount, double segmentLength, double radius, int resolution, double overlap)
		{
			var segments = new List<Solid>(segmentCount);
			for (var i = 0; i < segmentCount; i++)
			{
				var start = new Vector3D(0, 0, i * (segmentLength - overlap));
				var end = new Vector3D(0, 0, start.Z + segmentLength);
				segments.Add(Cylinder(new CylinderOptions
				{
					Start = start,
					End = end,
					RadiusStart = radius,
					RadiusEnd = radius,
					Resolution = resolution,
				}));
			}
			return segments;
		}

		[Test]
		public void OverlappingCylinderChain_UnionMatchesSingleSegmentSurfaceComplexity()
		{
			const int segmentCount = 40;
			var segments = BuildPipeChain(segmentCount, 1.0, 0.15, 12, 0.05);
			var singleSegmentTriangles = TriangleCount(segments[0]);

			Solid united = segments[0];
			for (var i = 1; i < segments.Count; i++)
			{
				united = united.Union(segments[i]);
			}

			var unionTriangles = TriangleCount(united);

			Assert.GreaterOrEqual(unionTriangles, singleSegmentTriangles - 4,
				"Co-linear overlapping cylinders should union into one shell without collapsing.");
			Assert.IsTrue(united.IsCanonicalized);
			Assert.IsTrue(united.IsRetesselated);
		}

		[TestCase(2)]
		[TestCase(10)]
		[TestCase(40)]
		[TestCase(83)]
		public void OverlappingCylinderChain_UnionDoesNotCollapse(int segmentCount)
		{
			var segments = BuildPipeChain(segmentCount, 1.0, 0.15, 12, 0.05);
			var singleSegmentTriangles = TriangleCount(segments[0]);
			var united = segments[0];
			for (var i = 1; i < segments.Count; i++)
			{
				united = united.Union(segments[i]);
			}

			Assert.GreaterOrEqual(TriangleCount(united), singleSegmentTriangles - 4);
		}
	}
}
