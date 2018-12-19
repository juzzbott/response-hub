﻿using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Messages;
using System;
using System.Collections.Generic;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Xps;

namespace Enivate.ResponseHub.Responsys.UI.Services
{
    public class PrintService
    {
		
		public void PrintJobReport(JobMessage job, string mapImagePath, Dictionary<string, Style> styles, IMessagePrintRecordRepository printRecordRepo)
		{

			FlowDocument flowDoc = (FlowDocument)Application.LoadComponent(new Uri("Assets/PrintedJobTemplate.xaml", UriKind.Relative));
            Section sctnHeader = new Section();
            sctnHeader.BorderThickness = new Thickness(0);

            Paragraph pghJobNumber = new Paragraph(new Run(job.JobNumber));
            pghJobNumber.Style = (job.Priority == MessagePriority.Emergency ? (Style)styles["ReportJobNumberEmergency"] : (Style)styles["ReportJobNumber"]);
            sctnHeader.Blocks.Add(pghJobNumber);

            Paragraph pghTimestamp = new Paragraph(new Run(job.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")));
            pghTimestamp.Style = (Style)styles["ReportTimestamp"];
            sctnHeader.Blocks.Add(pghTimestamp);

            Paragraph pghMapReference = new Paragraph(new Run(job.Location.MapReference));
            pghMapReference.Style = (Style)styles["ReportMapReference"];
            sctnHeader.Blocks.Add(pghMapReference);

            Paragraph pghAddress = new Paragraph(new Run(job.Location.Address.FormattedAddress));
            pghAddress.Style = (Style)styles["ReportAddress"];
            sctnHeader.Blocks.Add(pghAddress);

            Image imgMap = new Image();
            imgMap.Source = new BitmapImage(new Uri(mapImagePath, UriKind.Relative));
            imgMap.Style = (Style)styles["ReportMapImage"];
            InlineUIContainer imgContainer = new InlineUIContainer(imgMap);
            Paragraph pgMapImage = new Paragraph(imgContainer);
            pgMapImage.Style = (Style)styles["ReportMapImageContainer"];
            sctnHeader.Blocks.Add(pgMapImage);

            Paragraph pghMessageContent = new Paragraph(new Run(job.MessageContent));
            pghMessageContent.Style = (Style)styles["ReportMessageContent"];
            sctnHeader.Blocks.Add(pghMessageContent);

            flowDoc.Blocks.InsertBefore(flowDoc.Blocks.FirstBlock, sctnHeader);

            LocalPrintServer localPrintServer = new LocalPrintServer();
			PrintQueue printQueue = localPrintServer.DefaultPrintQueue;
			
			// Create a XpsDocumentWriter object, open a Windows common print dialog.
			// This methods returns a ref parameter that represents information about the dimensions of the printer media.
			XpsDocumentWriter docWriter = PrintQueue.CreateXpsDocumentWriter(printQueue);
			PageImageableArea ia = printQueue.GetPrintCapabilities().PageImageableArea;
			PrintTicket pt = printQueue.UserPrintTicket;
            //pt.CopyCount = 2;
			
			//if (docWriter != null && ia != null)
			//{
			//	DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDoc).DocumentPaginator;
            //
			//	// Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
			//	paginator.PageSize = new Size((double)ia.ExtentWidth, (double)ia.ExtentHeight);
			//	Thickness pagePadding = flowDoc.PagePadding;
            //    flowDoc.PagePadding = new Thickness(
			//	Math.Max(ia.OriginWidth, pagePadding.Left),
			//	Math.Max(ia.OriginHeight, pagePadding.Top),
			//	Math.Max((double)pt.PageMediaSize.Width - (double)(ia.OriginWidth + ia.ExtentWidth), pagePadding.Right),
			//	Math.Max((double)pt.PageMediaSize.Height - (double)(ia.OriginHeight + ia.ExtentHeight), pagePadding.Bottom));
            //    flowDoc.ColumnWidth = double.PositiveInfinity;
			//	// Send DocumentPaginator to the printer.
			//	docWriter.Write(paginator);
			//}

            // Add the print report
            Task t = Task.Run(async () =>
            {
                await printRecordRepo.CreatePrintRecord(job.Id, DateTime.UtcNow);
            });
            t.Wait();

        }
	}
}
