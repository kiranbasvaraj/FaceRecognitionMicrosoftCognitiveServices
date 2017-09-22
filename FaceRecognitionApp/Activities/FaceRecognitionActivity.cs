using System;

using Android.App;
using Android.OS;
using Android.Widget;
using FaceRecognitionApp.Helpers;
using Android.Graphics;
using System.IO;
using Android.Content;
using Android.Runtime;

namespace FaceRecognitionApp.Activities
{
    [Activity(Label = "FaceRecognitionActivity", MainLauncher = true)]
    public class FaceRecognitionActivity : Activity
    {
        public ImageView _intentFirstImage;
        public ImageView _IntentSecondImage;
        public ImageView _firstImage;
        public ImageView _secondImage;
        public Button _verifyButton;
        public Bitmap _firstImageBitmap { get; set; }
        public Bitmap _secondImageBitmap { get; set; }
        FaceRecognitionHelperTask _helper;
        ProgressDialog _pd;
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            _helper = new FaceRecognitionHelperTask();
            _pd = new ProgressDialog(this);
            _pd.Indeterminate = true;
            _pd.SetProgressStyle(ProgressDialogStyle.Spinner);
            _pd.SetMessage(" Please wait...");
            _pd.SetCancelable(false);
            _firstImageBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.gayle);
            _secondImageBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.young_sachin);
            FindViews();
            HandleEvents();

        }
        void FindViews()
        {
            _firstImage = FindViewById<ImageView>(Resource.Id.imageView1);
            _secondImage = FindViewById<ImageView>(Resource.Id.imageView2);
            _verifyButton = FindViewById<Button>(Resource.Id.verifyButton);
            _firstImage.SetImageBitmap(_firstImageBitmap);
            _secondImage.SetImageBitmap(_secondImageBitmap);

        }
        void HandleEvents()
        {
            _verifyButton.Click += _verifyButton_Click;
            _firstImage.Click += _firstImage_Click;
            _secondImage.Click += _secondImage_Click;
        }
        int type;
        private void _secondImage_Click(object sender, EventArgs e)
        {
            type = 2;
            var imageIntent = new Intent();
            imageIntent.SetType("image/*");
            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(
            Intent.CreateChooser(imageIntent, "Select photo"), 0);
        }

        private void _firstImage_Click(object sender, EventArgs e)
        {
            type = 1;
            var imageIntent = new Intent();
            imageIntent.SetType("image/*");
            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(
            Intent.CreateChooser(imageIntent, "Select photo"), 0);
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                Android.Net.Uri uri = data.Data;
                if (type == 1)
                {

                    _firstImageBitmap = Android.Provider.MediaStore.Images.Media.GetBitmap(ContentResolver, uri);
                    _firstImage.SetImageBitmap(_firstImageBitmap);
                }
                else
                {
                    _secondImageBitmap = Android.Provider.MediaStore.Images.Media.GetBitmap(ContentResolver, uri);
                    _secondImage.SetImageBitmap(_secondImageBitmap);
                }



            }
        }
        private async void _verifyButton_Click(object sender, EventArgs e)
        {
            _pd.Show();
            byte[] firstImageBitmapData;
            byte[] secondImageBitmapData;
            string _faceId1 = string.Empty;
            string _faceId2 = string.Empty;

            MemoryStream stream1 = new MemoryStream();
            _firstImageBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream1);
            firstImageBitmapData = stream1.ToArray();

            MemoryStream stream2 = new MemoryStream();
            _secondImageBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream2);
            secondImageBitmapData = stream2.ToArray();

            MemoryStream inputStream1 = new MemoryStream(firstImageBitmapData);
            MemoryStream inputStream2 = new MemoryStream(secondImageBitmapData);
            //if (_helper == null)
            //{
            //    _helper = new FaceRecognitionHelperTask();
                //}
                ////testcode
                //RestClient rest = new RestClient();
                //// FaceID face = new FaceID { }
                //PhotoStream p = new PhotoStream();
                //p.Url = inputStream1;

                //    var x = await rest.RestClientInstance.Post("https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false",p, "ce415703236641cba2424d8d941301e5");
                //   //testcode
                string id1 = await _helper.GetFaceId1(inputStream1);
                string id2 = await _helper.GetFaceId2(inputStream2);

                var result = await _helper.VerifyPerson(id1, id2);

                _pd.Dismiss();

                if (result.IsIdentical)
                {

                    Toast.MakeText(this, "persons are identical" + " and confidence is " + result.Confidence, ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(this, "not identical" + result.Confidence, ToastLength.Long).Show();
                }
           // }
        }
       public class FaceID
        {
           public string faceid1 { get; set; }
           public string faceid2 { get; set; }
        }

        public class PhotoStream
        {
           public Stream Url { get; set; }
        }
    }
}