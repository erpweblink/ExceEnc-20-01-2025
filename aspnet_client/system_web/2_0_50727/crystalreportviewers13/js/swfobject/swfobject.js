c.Add(table);
            ///end Total InNumber

            //Total amount In word
            table = new PdfPTable(3);
            table.TotalWidth = 560f;
            table.LockedWidth = true;

            paragraph.Alignment = Element.ALIGN_RIGHT;

            table.SetWidths(new float[] { 0f, 199f, 0f });
            table.AddCell(paragraph);

            PdfPCell cell44345 = new PdfPCell(new Phrase("Total Amount(Rs): " + Amtinword + " ONLY", FontFactory.GetFont("Arial", 9, Font.BOLD)));
            cell44345.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell44345);
            PdfPCell cell443457 = new PdfPCell(new Phrase("Total Amount(Rs): ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
            //cell443457.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell443457);
            PdfPCell cell44044 = new PdfPCell(new Phrase(FinaleTotalamt.ToString(), FontFactory.GetFont("Arial", 10, Font.BOLD)));
            cell44044.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell44044);
            doc.Add(table);
            ///end Total Amount

            //Declaration
            Paragraph paragraphTable99 = new Paragraph(" Remarks :\n\n", font12);

            //Puja Enterprises Sign
            string[] itemss = {
                "Declaration  : I/We hereby certify that my/our registration certificate under the GST Act, 2017 is in force on the date on which the supply of",
                "the goods specified in this tax invoice is made by me/us and that the transaction of supplies covered by this tax invoice has been effected",
                "by me/us and it shall be accounted for in the turnover of supplies while filing of return and the due tax, if any, payable on the supplies",
                "has been paid or shall be paid. \n",

                "Subject To Pune Jurisdiction Only.",

                        };

            Font font14 = FontFactory.GetFont("Arial", 9);
            Font font15 = FontFactory.GetFont("Arial", 9, Font.NORMAL);
            Paragraph paragraphhh = new Paragraph(" Terms & Condition :\n\n", font15);


            for (int i = 0; i < itemss.Length; i++)
            {
                //paragraphhh.Add(new Phrase("\u2022 \u00a0" + itemss[i] + "\n", font15));
                paragraphhh.Add(new Phrase(itemss[i] + "\n", font15));
            }

            table = new PdfPTable(1);
            table.TotalWidth = 560f;
            table.LockedWidth = true;
            table.SetWidths(new float[] { 560f });

            table.AddCell(paragraphhh);
            //table.AddCell(new Phrase("Puja Enterprises \n\n\n\n         Sign", FontFactory.GetFont("Arial", 10, Font.BOLD)));
            //table.AddCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.BOLD)));
            //doc.Add(table);
            ///end declaration
            ///

            ///Sign Authorization
            ///

            // Bind stamp Image
            string imageStamp = Server.MapPath("~") + "/img/Purchase.png";
            iTextSharp.text.Image image1 = iTextSharp.text.Image.GetInstance(imageStamp);
            image1.ScaleToFit(600, 120);
            PdfPCell imageCell = new PdfPCell(image1);
            imageCell.PaddingLeft = 10f;
            imageCell.PaddingTop = 0f;
            /////////////////

            Paragraph paragraphTable10000 = new Paragraph();


            string[] itemss4 = {
                "Payment Term     ",

                        };

            Font font144 = FontFactory.GetFont("Arial", 11);
            Font font155 = FontFactory.GetFont("Arial", 8);
            Paragraph paragraphhhhhff = new Paragraph();
            table = new PdfPTable(2);
            table.TotalWidth = 560f;
            table.LockedWidth = true;
            table.SetWidths(new float[] { 300f, 100f });

            //table.AddCell(paragraphhhhhff);
            table.AddCell(new Phrase("Terms & Condition :\n\n Declaration  : I/We hereby certify that my/our registration certificate under the GST Act, 2017 is in force on the date on which the supply of the goods specified in this tax invoice is made by me/us and that the transaction of supplies covered by this tax invoice has been effected by me/us and it shall be accounted for in the turnover of supplies while filing of return and the due tax, if any, payable on the supplies has been paid or shall be paid. \n\n Subject To Pune Jurisdiction Only. \n\n\b Remark : " + Remark + "\n ", FontFactory.GetFont("Arial", 9)));
            table.AddCell(imageCell);
            table.AddCell(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
            doc.Add(table);
            //doc.Close();
            ///end Sign Authorization


            //doc.NewPage();

            //doc.Add(table);//Add the paragarh to the document  
        }

        //Byte[] FileBuffer = File.ReadAllBytes(Server.MapPath("~/files/") + "TaxInvoice.pdf");

        //Font blackFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK);
        //using (MemoryStream stream = new MemoryStream())
        //{
        //    PdfReader reader = new PdfReader(FileBuffer);
        //    using (PdfStamper stamper = new PdfStamper(reader, stream))
        //    {
        //        int pages = reader.NumberOfPages;
        //        for (int i = 1; i <= pages; i++)
        //        {
        //            if (i == 1)
        //            {

        //            }
        //            else
        //            {
        //                var pdfbyte = stamper.GetOverContent(i);
        //                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageURL);
        //                image.ScaleToFit(70, 100);
        //                image.SetAbsolutePosition(40, 792);
        //                image.SpacingBefore = 50f;
        //                image.SpacingAfter = 1f;
        //                image.Alignment = Element.ALIGN_LEFT;
        //                pdfbyte.AddImage(image);
        //            }
        //            var PageName = "Page No. " + i.ToString();
        //            ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase(PageName, blackFont), 568f, 820f, 0);
        //        }
        //    }
        //    FileBuffer = stream.ToArray();
        //}
        doc.Close();

        ifrRight6.Attributes["src"] = @"../files/" + Docname;
    }

    public static string ConvertNumbertoWords(int number)
    {
        if (number == 0)
            return "ZERO";
        if (number < 0)
            return "minus " + ConvertNumbertoWords(Math.Abs(number));
        string words = "";
        if ((number / 1000000) > 0)
        {
            words += ConvertNumbertoWords(number / 1000000) + " MILLION ";
            number %= 1000000;
        }
        if ((number / 1000) > 0)
        {
            words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
            number %= 1000;
        }
        if ((number / 100) > 0)
        {
            words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
            number %= 100;
        }
        if (number > 0)
        {
            if (words != "")
                words += "AND ";
            var unitsMap = new[] { "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
            var tensMap = new[] { "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += " " + unitsMap[number % 10];
            }
        }
        return words;
    }

    protected void btnOriginal_Click(object sender, EventArgs e)
    {
        Pdf("Original");
    }

    protected void btnDuplicate_Click(object sender, EventArgs e)
    {
        Pdf("Duplicate");
    }

    protected void btnTriplicate_Click(object sender, EventArgs e)
    {
        Pdf("Triplicate");
    }

    protected void btnExtra_Click(object sender, EventArgs e)
    {
        Pdf("Extra");
    }
}                                                                                                                                                                                                                                                                                                                                                                            