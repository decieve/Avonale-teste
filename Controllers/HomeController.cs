using Avonale_teste.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Avonale_teste.DAO;

namespace Avonale_teste.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HttpClient _httpClient;
        public HomeController(ILogger<HomeController> logger)
        {
            //Inicialização do httpclient para as requisições
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "c# App");
        }

        public IActionResult Index()
        {
            return View();
        }

        //Retorna repositórios do autor
        [Route("MyRepos")]
        [HttpGet("MyRepos")]
        public async  Task<IActionResult> MyRepos()
        {   
            var response = await _httpClient.GetAsync("users/decieve/repos");
            List<Repositorio> repositorios = new List<Repositorio>();
            Console.WriteLine(response.Content);
            if (response.IsSuccessStatusCode)
            {
                
                string data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Repositorio[]>(data);
                repositorios = new List<Repositorio>(result);
               
            } 
            ViewData["Repos"] = repositorios;
            return View();
        }
        /**
         * Método para Pesquisar repositórios
         * 
         */
        [Route("SearchRepos")]
        [HttpGet("SearchRepos")]
        public async Task<IActionResult> SearchRepos(string search,int page)
        {

            var response = await _httpClient.GetAsync("search/repositories?per_page=20&page="+page+"&q=" + search);
            List<Repositorio> repositorios = new List<Repositorio>();

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data).SelectToken("items").ToString();
                string total_count = JObject.Parse(data).SelectToken("total_count").ToString();
                var result = JsonConvert.DeserializeObject<Repositorio[]>(obj);
                repositorios = new List<Repositorio>(result);
                ViewData["total_count"] = int.Parse(total_count);
            }
            ViewData["Repos"] = repositorios;
            ViewData["SearchString"] = search;
            ViewData["Page"] = page;
            return View();
        }
        /**
         * Método para retornar as informações do repositório especificado nos parametros "user" e "reponame"
         * Retorna repositório com contribuidores e linguagens de programação utilizadas
         * Possui um parametro enviado em formato query para mensagens de sucesso ao favoritar
         */
        [Route("Repo")]
        [HttpGet("Repo/{user}/{reponame}")]
        public async Task<IActionResult> Repo(string user,string reponame,string message)
        {

            var responseRepo = await _httpClient.GetAsync("repos/" + user +"/"+ reponame);

            // Realiza a obtenção das informações do repositório , e caso seja bem-sucedido prossegue
            if (responseRepo.IsSuccessStatusCode) {
                string json_repo = await responseRepo.Content.ReadAsStringAsync();
                Repositorio repo = JsonConvert.DeserializeObject<Repositorio>(json_repo);
                // Obtenção de linguagens do repositório
                var responseLanguages = await _httpClient.GetAsync("repos/" + user + "/" + reponame + "/languages");
                if (responseLanguages.IsSuccessStatusCode)
                {
                    //Transformação dos valores do nome do atributo em String ( dicionário com key(nome do atributo) + valor arbitrario do atributo )
                    string resposta = await responseLanguages.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(resposta);
                    IDictionary<string, JToken> languagesDict = json;
                    List<string> languages = new List<string>();
                    foreach (var lang in languagesDict)
                    {
                        languages.Add(lang.Key);
                    }
                    repo.Languages = languages;
                }
                // Obtenção de Contribuidores do repositório
                var responseContributors = await _httpClient.GetAsync("repos/" + user + "/" + reponame + "/contributors");
                if (responseContributors.IsSuccessStatusCode){
                    string resposta = await responseContributors.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<User[]>(resposta);
                    repo.Contributors = new List<User>(users);
                }
                ViewData["Repo"] = repo;
                ViewData["MessageDB"] = message;
            }
            
            return View();
        }
        // Realiza uma requisição ao banco para retornar os repositórios favoritos
        [Route("Favoritos")]
        public IActionResult Favoritos()
        {
            List<Repositorio> repos = FavoritosDAO.GetFavoritos();

            ViewData["Repos"] = repos;
            return View();
        }
        // Favorita um repositório
        [Route("Favoritar")]
        [HttpPost("Favoritar")]
        public IActionResult Favoritar(string repo,string username,string avatarurl) {

            string  messageDB = FavoritosDAO.InserirFavorito(repo, username, avatarurl) ? "Adicionado aos favoritos com sucesso" : "Este repositório já esta marcado como favorito";
            return RedirectToAction("Repo", "Home", new { user = username, reponame = repo , message = messageDB});
        }
        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
