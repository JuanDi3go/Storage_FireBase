using Firebase.Auth;

using Firebase.Storage;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Storage_FireBase.Models;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Storage_FireBase.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string cadenaSQL;
        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
            cadenaSQL = configuration.GetConnectionString("DbConnection");
        }


        public IActionResult Index()
        {
            var oListEmployee =  new List<EmployeeModel>();

            using (var con = new SqlConnection(cadenaSQL))
            {
                con.Open();
                var cmd = new SqlCommand("ListEmpleado", con);


                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        oListEmployee.Add(new EmployeeModel { 
                        Id = int.Parse(dr["IdEmployee"].ToString()),
                        Name = dr["NameEmployee"].ToString(),
                        PhoneNumber = dr["Phone"].ToString(),
                        UrlPicture = dr["UrlPicture"].ToString(),
                        });
                    }
                }

            }

            return View(oListEmployee);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeModel oEmpleado, IFormFile image)
        {
            Stream imageS = image.OpenReadStream();
            string ulrImage = await SubirStorage(imageS, image.FileName);

            using (var con = new SqlConnection(cadenaSQL))
            {
                con.Open();
                var cmd = new SqlCommand("SaveEmployee", con);
                cmd.Parameters.AddWithValue("@NameEmployee", oEmpleado.Name);
                cmd.Parameters.AddWithValue("@PhoneEmployee", oEmpleado.PhoneNumber);
                cmd.Parameters.AddWithValue("@UrlImage", ulrImage);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }


            return RedirectToAction("Index");
        }


        public async Task<string> SubirStorage(Stream picture, string name)
        {
            string email = "FirebaseMail@gmail.com";
            string password = "juan123456";
            string rute = "urlFirebase";
            string apiKey = "yourApiKey";

         
         var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));

            var a = await auth.SignInWithEmailAndPasswordAsync(email, password);

            var cancellation =  new CancellationTokenSource();

            var task = new FirebaseStorage(rute,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true,

                }).Child("ProfilePicture").Child(name).PutAsync(picture, cancellation.Token);

            var downloadUrl = await task;

            return downloadUrl;
        }
    }
}
