using System;
using EdiEngine.Standards.X12_004010.Maps;
using System.Linq;
using EdiEngine;
using EdiEngine.Common.Definitions;
using EdiEngine.Runtime;
using SegmentDefinitions = EdiEngine.Standards.X12_004010.Segments;
using Newtonsoft.Json;
using WriteFile.Data;

namespace WriteFile
{
    class Program
    {
        static void Main(string[] args)
        {

            Write271File("");
      

        }

        private static string ReadEditoJson(string edimessage)
        {
           
                string edi =
                    @"ISA*01*0000000000*01*0000000000*ZZ*ABCDEFGHIJKLMNO*ZZ*123456789012345*101127*1719*U*00400*000003438*0*P*>
                GS*OW*7705551212*3111350000*20000128*0557*3317*T*004010
                ST*940*0001
                W05*N*538686**001001*538686
                LX*1
                W01*12*CA*000100000010*VN*000100*UC*DEC0199******19991205
                G69*11.500 STRUD BLUBRY
                W76*56*500*LB*24*CF
                SE*7*0001
                GE*1*3317
                IEA*1*000003438";

                EdiDataReader r = new EdiDataReader();
                EdiBatch b = r.FromString(edi);

                //Serialize the whole batch to JSON
                JsonDataWriter w1 = new JsonDataWriter();
                string json = w1.WriteToString(b);

                //Serialize selected EDI message to Json
                string jsonTrans = JsonConvert.SerializeObject(b.Interchanges[0].Groups[0].Transactions[0]);
                            
            return json;

        }


        private static string Write271File(string ediFile) 
        {
            M_271 map = new M_271();
            EdiTrans t = new EdiTrans(map);

            // BHT
            var sDef = (MapSegment)map.Content.First(s => s.Name == "BHT");

            var seg = new EdiSegment(sDef);
            seg.Content.AddRange(new[] {
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[0], "ZZZZ"),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[1], "ZZ"),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[2], null),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[3],  DateTime.Now.ToString("yyyyMMdd")),
                new EdiSimpleDataElement((MapSimpleDataElement)sDef.Content[4], DateTime.Now.ToString("HHmmss"))
            });

            t.Content.Add(seg);

            //HL loop
            var lDef = (MapLoop)map.Content.First(s => s.Name == "L_HL");
            sDef = (MapSegment)lDef.Content.First(s => s.Name == "HL");
            
            EdiLoop HL = new EdiLoop(lDef, null);       
            t.Content.Add(HL);

            seg = new EdiSegment(sDef);
            seg.Content.AddRange(new[]
            {
                new EdiSimpleDataElement(sDef.Content[0], "1"),
                new EdiSimpleDataElement(sDef.Content[1], "1"),
                new EdiSimpleDataElement(sDef.Content[2], "ZZ")
            });
            HL.Content.Add(seg);

            //TRN loop
            lDef = (MapLoop)map.Content.First(s => s.Name == "L_HL");
            sDef = (MapSegment)lDef.Content.First(s => s.Name == "TRN");

            EdiLoop TRN = new EdiLoop(lDef, null);
            t.Content.Add(TRN);

            seg = new EdiSegment(sDef);
            seg.Content.AddRange(new[]
            {
                new EdiSimpleDataElement(sDef.Content[0], "1"),
                new EdiSimpleDataElement(sDef.Content[1], "1"),
                new EdiSimpleDataElement(sDef.Content[2], "1234567890"),
                new EdiSimpleDataElement(sDef.Content[3], "ZZ")
            });
            TRN.Content.Add(seg);

            //AAA loop
            lDef = (MapLoop)map.Content.First(s => s.Name == "L_HL");
            sDef = (MapSegment)lDef.Content.First(s => s.Name == "AAA");

            EdiLoop AAA = new EdiLoop(lDef, null);
            t.Content.Add(AAA);

            seg = new EdiSegment(sDef);
            seg.Content.AddRange(new[]
            {
                new EdiSimpleDataElement(sDef.Content[0], "Y"),
                new EdiSimpleDataElement(sDef.Content[1], "ZZ"),
                new EdiSimpleDataElement(sDef.Content[2], "ZZ"),
                new EdiSimpleDataElement(sDef.Content[3], "Y")
            });

            AAA.Content.Add(seg);


            //NM1 loop
            lDef = (MapLoop)lDef.Content.First(s => s.Name == "L_NM1");
            sDef = (MapSegment)lDef.Content.First(s => s.Name == "NM1");



            using (var context = new Data.DataSetTableAdapters.MemberTableAdapter() )
            {
               var data = context.GetData();

                foreach (var d in data)
                {
                    
                    EdiLoop NM1 = new EdiLoop(lDef, null);
                    t.Content.Add(NM1);
                    seg = new EdiSegment(sDef);

                    seg.Content.AddRange(new[]
                        {
                            new EdiSimpleDataElement(sDef.Content[0], "ZZ"),
                            new EdiSimpleDataElement(sDef.Content[1], "L"),
                            new EdiSimpleDataElement(sDef.Content[2], "Hummana"),
                            new EdiSimpleDataElement(sDef.Content[3], d.FirstName),
                            new EdiSimpleDataElement(sDef.Content[4], d.LastName),
                            new EdiSimpleDataElement(sDef.Content[5], " "),
                            new EdiSimpleDataElement(sDef.Content[6], " "),
                            new EdiSimpleDataElement(sDef.Content[7], "ZZ"),
                            new EdiSimpleDataElement(sDef.Content[8], d.MHKMemberInternalID.ToString()),
                            new EdiSimpleDataElement(sDef.Content[9], "97"),
                            new EdiSimpleDataElement(sDef.Content[10], "ZZ")
                        });
                   
                    NM1.Content.Add(seg);

                }
                
            }
               

            /*Update Edi Group*/
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
                "ZZ", "SENDER", "ZZ", "RECEIVER", "GSSENDER", "GSRECEIVER", "00401", "004010", "T", 100, 200, "\r\n", "*");

            EdiDataWriter w = new EdiDataWriter(settings);
            Console.WriteLine(w.WriteToString(b));
            Console.Read();

            return "";
        }

    }

}
