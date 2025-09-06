using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using static System.Net.Mime.MediaTypeNames;

namespace GMSApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } =  string.Empty ;


        public bool IsAuthenticated { get; set; }
    }
}




