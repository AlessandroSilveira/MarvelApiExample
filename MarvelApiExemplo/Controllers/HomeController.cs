using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MarvelApiExemplo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices]IConfiguration config)
        {
            Personagem personagem;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string ts = DateTime.Now.Ticks.ToString();
                string publicKey = config.GetSection("MarvelComicsAPI:PublicKey").Value;
                string hash = GerarHash(ts, publicKey,
                    config.GetSection("MarvelComicsAPI:PrivateKey").Value);

                HttpResponseMessage response = client.GetAsync(config.GetSection("MarvelComicsAPI:BaseUrl").Value +
                                                               $"characters?ts={ts}&apikey={publicKey}&hash={hash}&" +
                                                               $"limit=20").Result;
                response.EnsureSuccessStatusCode();
                string conteudo = response.Content.ReadAsStringAsync().Result;

                dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                List<Personagem> personagens = new List<Personagem>();

                for (int i = 0; i < resultado.data.results.Count; i++)
                {
                    personagens.Add(new Personagem
                    {
                        Nome = resultado.data.results[i].name,
                        Descricao = resultado.data.results[i].description,
                        UrlImagem = resultado.data.results[i].thumbnail.path + "." +
                                     resultado.data.results[i].thumbnail.extension,
                    });
                }
                //personagem = new Personagem
                //{
                // Nome = resultado.data.results[0].name,
                // Descricao = resultado.data.results[0].description,
                // UrlImagem = resultado.data.results[0].thumbnail.path + "." +
                //             resultado.data.results[0].thumbnail.extension,
                // UrlWiki = resultado.data.results[0].urls[1].url
                //};


                ViewBag.ListaPersonagens = personagens;
               
            }
            return View();
            // return View(personagem);

        }

        private string GerarHash(string ts, string publicKey, string privateKey)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash).ToLower().Replace("-", String.Empty);
        }


    }
}