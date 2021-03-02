using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using Avonale_teste.Models;

namespace Avonale_teste.DAO
{
    public class FavoritosDAO
    {
        private static SQLiteConnection conexao;
        
        private static SQLiteConnection DbConnection()
        {
            conexao = new SQLiteConnection("Data Source=avonale.db;Version=3");
            conexao.Open();
            return conexao;
        }
        // Insere Favorito no banco de dados SQLITE
        public static bool InserirFavorito(string nome_repo,string user_name,string avatar_link)
        {
            try
            {
                var command = DbConnection().CreateCommand();
                if (!Exists(command,user_name, nome_repo))
                {
                    
                    command.CommandText = "INSERT INTO favoritos(repo,user,avatar_url) VALUES(@nome_repo,@user_name,@avatar_link)";
                    command.Parameters.AddWithValue("@nome_repo", nome_repo);
                    command.Parameters.AddWithValue("@user_name", user_name);
                    command.Parameters.AddWithValue("@avatar_link", avatar_link);
                    command.ExecuteNonQuery();
                    return true;
                }else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        // Retorna true caso repositório exista no banco
        private static bool Exists(SQLiteCommand command,string user,string repo)
        {
            try
            {
                command.CommandText = "SELECT EXISTS ( SELECT * FROM favoritos WHERE repo = @repo AND user = @user)";
                command.Parameters.AddWithValue("@repo", repo);
                command.Parameters.AddWithValue("@user", user);
                bool existe = true;
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    existe = reader.GetBoolean(0);
                }
                command.Reset();
                return existe;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        // Retorna repositórios favoritos
        public static List<Repositorio> GetFavoritos()
        {
            List<Repositorio> lista = new List<Repositorio>();
            try
            {
                var command = DbConnection().CreateCommand();
                command.CommandText = "SELECT * FROM favoritos";
                using (var reader = command.ExecuteReader()) {

                    while (reader.Read()) {
                        Repositorio r = new Repositorio();
                        r.Owner = new User();
                        r.Id = reader.GetInt32(0);
                        r.Name = reader.GetString(1);
                        r.Owner.Login = reader.GetString(2);
                        r.Owner.Avatar_url = reader.GetString(3);
                        lista.Add(r);
                    }
                }
                return lista;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
