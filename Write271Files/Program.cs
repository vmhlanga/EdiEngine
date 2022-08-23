using System;
using EdiEngine.Standards.X12_004010.Maps;
using System.Linq;
using EdiEngine;
using EdiEngine.Common.Definitions;
using EdiEngine.Runtime;
using SegmentDefinitions = EdiEngine.Standards.X12_004010.Segments;

namespace Write271Files
{
    class Program
    {
        static void Main(string[] args)
        {
            M_270 map = new M_M_271();
            EdiTrans t = new EdiTrans(map);

            // W05
            var sDef = (MapSegment)map.Content.First(s => s.Name == "W05");

            var seg = new EdiSegment(sDef);
            seg.Content.AddRange(new[] {
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[0], "N"),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[1], "538686"),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[2], null),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[3], "001001"),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[4], "538686")
            });

            t.Content.Add(seg);

            var g = new EdiGroup("OW");
            g.Transactions.Add(t);

            var i = new EdiInterchange();
            i.Groups.Add(g);

            EdiBatch b = new EdiBatch();
            b.Interchanges.Add(i);

            //Add all service segments
            EdiDataWriterSettings settings = new EdiDataWriterSettings(
                new SegmentDefinitions.ISA(), new SegmentDefinitions.IEA(),
                new SegmentDefinitions.GS(), new SegmentDefinitions.GE(),
                new SegmentDefinitions.ST(), new SegmentDefinitions.SE(),
                "ZZ", "SENDER", "ZZ", "RECEIVER", "GSSENDER", "GSRECEIVER",
                "00401", "004010", "T", 100, 200, "\r\n", "*");

            EdiDataWriter w = new EdiDataWriter(settings);
            Console.WriteLine(w.WriteToString(b));
            Console.Read();
        }
    }
}
