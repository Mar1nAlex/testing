using _4WebApp_12._01_.Models;
using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace _4WebApp_12._01_.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string pattern)
        {

            List<SearchResultLine> result;
            using (HotelEntities db = new HotelEntities())
            {
                result = db.Guests
                    .Join(db.GuestsNumbers,g => g.Id,gn => gn.Guest_Id,
                    (g, gn) => new
                        {
                            Name = g.Name,
                            Surname = g.Surname,
                            id_g_n = gn.Id,
                            id_g = gn.Number_Id
                        }
                   ).Join(db.Numbers, gn => gn.id_g,n => n.Id,
                    (gn, n) => new SearchResultLine()
                        {
                            Name = gn.Name,
                            Surname = gn.Surname,
                            HotelNumber = n.Name,
                            GuestNumber = n.Number.Value.ToString() 
                        }
                    ).ToList();
            }
            if (pattern == null)
            {
                ViewBag.SearchData = result;
                return View();
            }
            else
            {
                result = result.Where((p) => p.Name.Contains(pattern)).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

    

        public ActionResult AddGuests(string name, string surname)
        {
            using (HotelEntities db = new HotelEntities())
            {
                Guests p = new Guests() { Name = name, Surname = surname };

                db.Guests.Add(p);
                db.SaveChanges();
            }

            List<Tuple<int, string, string>> result = new List<Tuple<int, string, string>>();
            using (HotelEntities db = new HotelEntities())
            {
                var Guest = db.Guests.ToList();
                foreach (var p in Guest)
                    result.Add(Tuple.Create(
                        p.Id,
                        p.Name,
                        p.Surname)
                        );
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RemoveGuest(int id)//, string name, string surname
        {

            using (HotelEntities db = new HotelEntities())
            {
                 Guests guests = db.Guests.Find(id);
               // Guests guests = db.Guests.First(a => a.Name.Equals(name) && b => b.Surname.Equals(surname) && c => c.Id.Equals(id));
                db.Guests.Remove(guests);
                db.SaveChanges();
            }

           List<Tuple<int, string, string>> result = new List<Tuple<int, string, string>>();
            using (HotelEntities db = new HotelEntities())
            {
                var Guest = db.Guests.ToList();
                foreach (var p in Guest)
                    result.Add(Tuple.Create(
                        p.Id,
                        p.Name,
                        p.Surname)
                        );
            }
            
            return Json(result, JsonRequestBehavior.AllowGet);

        }
   
        // удаление
        /* if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Guests guests = db.Guests.Find(id);
            if (Guests == null)
            {
                return HttpNotFound();
            }
            return View(guests);
            */

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private MemoryStream GenerateWord(string[,] data)
        {
            MemoryStream mStream = new MemoryStream();
            // Создаем документ
            WordprocessingDocument document =
                WordprocessingDocument.Create(mStream, WordprocessingDocumentType.Document, true);
            // Добавляется главная часть документа. 
            MainDocumentPart mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());
            // Создаем таблицу. 
            Table table = new Table();
            body.AppendChild(table);

            // Устанавливаем свойства таблицы(границы и размер).
            TableProperties props = new TableProperties(
                new TableBorders(
                new TopBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new BottomBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new LeftBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new RightBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new InsideHorizontalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new InsideVerticalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                }));

            // Назначаем свойства props объекту table
            table.AppendChild<TableProperties>(props);

            // Заполняем ячейки таблицы.
            for (var i = 0; i <= data.GetUpperBound(0); i++)
            {
                var tr = new TableRow();
                for (var j = 0; j <= data.GetUpperBound(1); j++)
                {
                    var tc = new TableCell();
                    tc.Append(new Paragraph(new Run(new Text(data[i, j]))));

                    // размер колонок определяется автоматически.
                    tc.Append(new TableCellProperties(
                        new TableCellWidth { Type = TableWidthUnitValues.Auto }));

                    tr.Append(tc);
                }
                table.Append(tr);
            }

            mainPart.Document.Save();
            //document.Close();
            document.Clone();
            mStream.Position = 0;
            return mStream;
        }


        public FileStreamResult GetWord()
        {
            using (HotelEntities db = new HotelEntities())
            {
                List<SearchResultLine> result;

                result = db.Guests
                .Join(db.GuestsNumbers, g => g.Id, gn => gn.Guest_Id,
                (g, gn) => new
                {
                    Name = g.Name,
                    Surname = g.Surname,
                    id_g_n = gn.Id,
                    id_g = gn.Number_Id
                }
               ).Join(db.Numbers, gn => gn.id_g, n => n.Id,
                (gn, n) => new SearchResultLine()
                {
                    Name = gn.Name,
                    Surname = gn.Surname,
                    HotelNumber = n.Name,
                    GuestNumber = n.Number.Value.ToString()
                }
                ).ToList();

                string[,] data = new string[result.Count(), 4];
                int i = 0;
                foreach (var s in result)
                {
                    data[i, 0] = s.Name.ToString();
                    data[i, 1] = s.Surname;
                    data[i, 2] = s.HotelNumber;
                    data[i, 3] = s.GuestNumber;
                    i++;
                }
                MemoryStream memoryStream = GenerateWord(data);
                memoryStream.Position = 0;
                return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = "demo.docx"
                };
            }

        }


        // Create a Paragraph with justification
        public FileStreamResult GetWord4()
        {
            // Create Stream
            MemoryStream mem = new MemoryStream();
            // Create Document
            using (WordprocessingDocument wordDocument =
                WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                // Add a main document part. 
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                // Create the document structure and add some text.
                mainPart.Document = new Document();
                Body docBody = new Body();
                mainPart.Document.AppendChild(docBody);

                // Add your docx content here
                Paragraph p = new Paragraph();
                ParagraphProperties pp = new ParagraphProperties();
                pp.Justification = new Justification() { Val = JustificationValues.Center };
                // Add paragraph properties to your paragraph
                p.Append(pp);
                // Run
                Run r = new Run();
                Text t = new Text("Nam eu tortor ut mi euismod eleifend in ut ante. Donec a ligula ante. Sed rutrum ex quam. Nunc id mi ultricies, vestibulum sapien vel, posuere dui.") { Space = SpaceProcessingModeValues.Preserve };
                r.Append(t);
                p.Append(r);
                // Add your paragraph to docx body
                docBody.Append(p);

                mainPart.Document.Save();
            }
            mem.Position = 0;
            return new FileStreamResult(mem, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = "demo.doc"
            };
        }


        // Create Table
        public FileStreamResult GetWord5()
        {
            // Create Stream
            MemoryStream mem = new MemoryStream();
            // Create Document
            using (WordprocessingDocument wordDocument =
                WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                // Add a main document part. 
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                // Create the document structure and add some text.
                mainPart.Document = new Document();
                Body docBody = new Body();
                mainPart.Document.AppendChild(docBody);

                // Add your docx content here
                Table table = new Table();

                /* ROW #1 */
                TableRow tr1 = new TableRow();

                TableCell tc11 = new TableCell();
                Paragraph p11 = new Paragraph(new Run(new Text("A")));
                tc11.Append(p11);
                tr1.Append(tc11);

                TableCell tc12 = new TableCell();
                Paragraph p12 = new Paragraph();
                Run r12 = new Run();
                RunProperties rp12 = new RunProperties();
                rp12.Bold = new Bold();
                r12.Append(rp12);
                r12.Append(new Text("Nice"));
                p12.Append(r12);
                tc12.Append(p12);

                tr1.Append(tc12);
                table.Append(tr1);


                /* ROW #2 */
                TableRow tr2 = new TableRow();

                TableCell tc21 = new TableCell();
                Paragraph p21 = new Paragraph(new Run(new Text("Little")));
                tc21.Append(p21);
                tr2.Append(tc21);

                TableCell tc22 = new TableCell();
                Paragraph p22 = new Paragraph();
                ParagraphProperties pp22 = new ParagraphProperties();
                pp22.Justification = new Justification() { Val = JustificationValues.Center };
                p22.Append(pp22);
                p22.Append(new Run(new Text("Table")));
                tc22.Append(p22);
                tr2.Append(tc22);

                table.Append(tr2);


                // Add your table to docx body
                docBody.Append(table);

                mainPart.Document.Save();
            }
            mem.Position = 0;
            return new FileStreamResult(mem, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = "demo.doc"
            };
        }


    }
}