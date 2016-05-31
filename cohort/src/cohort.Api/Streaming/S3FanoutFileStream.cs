namespace cohort.API.Streaming
{
    public class S3FanoutFileStream
    {
        public void FinishPart()
        {
            // As soon as a demarcated part has all bytes, start this process (in a serial queue?)
            //var utility = new TransferUtility();
            //var request = new TransferUtilityUploadRequest()
            //        .WithBucketName(BucketName)
            //        .WithFilePath(sourceFile.FullName)
            //        .WithKey(key)
            //        .WithTimeout(100 * 60 * 60 * 1000)
            //        .WithPartSize(10 * 1024 * 1024)
            //        .WithSubscriber((src, e) =>
            //        {
            //            Console.CursorLeft = 0;
            //            Console.Write("{0}: {1} of {2}    ", sourceFile.Name, e.TransferredBytes, e.TotalBytes);
            //        });
            //    utility.Upload(request);    
        }
    }
}