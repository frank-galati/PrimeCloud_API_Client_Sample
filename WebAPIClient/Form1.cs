using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebAPIClient.Helper;

namespace WebAPIClient
{
    public partial class Form1 : Form
    {
        string xtoken = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string encodedUserCredentials =
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(textBox1.Text.Trim() + ":" + textBox2.Text.Trim() + ":" + textBox3.Text.Trim()));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(txtserver.Text +"/api/primecloud/Authenticate");
                request.Accept = "application/json";
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("Authorization", "Basic " + encodedUserCredentials);

                HttpWebResponse response;
                using (Stream output = request.GetRequestStream())
                {
                    response = request.GetResponse() as HttpWebResponse;
                }
                request.Abort(); 

                richTextBox1.Text = "";
                richTextBox1.Text = response.Headers.ToString();

                xtoken = response.Headers["token"];
            }
            catch (WebException err)
            {
                using (WebResponse response = err.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    richTextBox1.Text = String.Format("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        // text is the response body
                        richTextBox1.Text = richTextBox1.Text + "\n" + reader.ReadToEnd();
                    }
                }
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader sr = new
                   System.IO.StreamReader(openFileDialog1.FileName);
                sr.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(xtoken)){
                    // text is the response body
                    richTextBox2.Text = "Empty or Invalid Token";
                    return;
                }

                // Generate post objects
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                postParameters.Add("filename", "xml.doc");
                postParameters.Add("fileformat", "xml");
                postParameters.Add("file", new FormUpload.FileParameter(File.ReadAllBytes(openFileDialog1.FileName), System.IO.Path.GetFileName(openFileDialog1.FileName), "application/xml"));


                try
                {
                    var httpWebRequest = FormUpload.MultipartFormDataPost(txtserver.Text + "/api/incorp/postxml", xtoken, postParameters);
                    using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        richTextBox2.Text = "Status Code: " + response.StatusCode + "\n Status Desc: " + response.StatusDescription + "\n" + response.ToString();
                    }
                    httpWebRequest.Abort();
                }
                catch (WebException err)
                {
                    using (WebResponse response = err.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        richTextBox2.Text = String.Format("Status code:{0} \nError code: {1}", httpResponse.StatusDescription, httpResponse.StatusCode);
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            // text is the response body
                            richTextBox2.Text = richTextBox2.Text + "\n" + reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                // text is the response body
                richTextBox2.Text = err.Message;
            }
        }
    }
}
