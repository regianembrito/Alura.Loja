using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alura.Loja.Testes.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //AdicionaCompras(4002, 3);
            //AdicionaCompras(4002, 1);
            //AdicionaCompras(4002, 5);
            //AdicionaCompras(4002, 10);
            using (var contexto = new LojaContext())
            {
                var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(SqlLoggerProvider.Create());

                var cliente = contexto
                    .Clientes
                    .Include(c => c.EnderecoDeEntrega)
                    .FirstOrDefault();

                Console.WriteLine($"Endereço de entrega: {cliente.EnderecoDeEntrega.Logradouro}");

                var produto = contexto
                    .Produtos
                    .Include(p => p.Compras)
                    .Where(p => p.Id == 4002)
                    .FirstOrDefault();

                contexto.Entry(produto)
                    .Collection(p => p.Compras)
                    .Query()
                    .Where(c => c.Preco > 10)
                    .Load();

                Console.WriteLine($"Mostrando as compras do produto {produto.Nome}");
                foreach (var item in produto.Compras)
                {
                    Console.WriteLine("\t" + item.Preco);
                }
            }
        }

        private static void ExibeProdutosDaPromocao()
        {
            using (var contexto = new LojaContext())
            {
                var promocao = contexto
                    .Promocoes
                    .Include(p => p.Produtos)
                    .ThenInclude(pp => pp.Produto)
                    .FirstOrDefault();

                Console.WriteLine("\nMostrando os produtos da promoção...");
                foreach (var item in promocao.Produtos)
                {
                    Console.WriteLine(item.Produto);
                }
            }
        }

        private static void FiltroPorCategoria()
        {
            using (var contexto = new LojaContext())
            {
                var promocao = new Promocao();
                promocao.Descricao = "Queima Total";
                promocao.DataInicio = new DateTime(2020, 1, 1);
                promocao.DataTermino = new DateTime(2020, 1, 31);

                var produtos = contexto
                    .Produtos
                    .Where(p => p.Categoria == "Bebidas")
                    .ToList();

                foreach (var item in produtos)
                {
                    promocao.IncluirProduto(item);
                }

                contexto.Promocoes.Add(promocao);
                ExibeEntries(contexto.ChangeTracker.Entries());
                contexto.SaveChanges();
            }
        }

        private static void UmParaUm()
        {
            var fulano = new Cliente();
            fulano.Nome = "Fulano de Tal";
            fulano.EnderecoDeEntrega = new Endereco()
            {
                Numero = 12,
                Logradouro = "Rua dos Heróis",
                Complemento = "Sobrado",
                Bairro = "Centro",
                Cidade = "Cidade"
            };

            using (var contexto = new LojaContext())
            {
                var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(SqlLoggerProvider.Create());

                contexto.Clientes.Add(fulano);
                contexto.SaveChanges();
            }
        }

        private static void MuitosParaMuitos()
        {
            var p1 = new Produto()
            {
                Nome = "Suco de Laranja",
                Categoria = "Bebidas",
                PrecoUnitario = 8.79,
                Unidade = "Litros"
            };
            var p2 = new Produto()
            {
                Nome = "Café",
                Categoria = "Bebidas",
                PrecoUnitario = 11.99,
                Unidade = "Gramas"
            };
            var p3 = new Produto()
            {
                Nome = "Macarrão",
                Categoria = "Alimentos",
                PrecoUnitario = 5.50,
                Unidade = "Gramas"
            };

            var promocaoDePascoa = new Promocao();
            promocaoDePascoa.Descricao = "Páscoa Feliz";
            promocaoDePascoa.DataInicio = DateTime.Now;
            promocaoDePascoa.DataTermino = DateTime.Now.AddMonths(3);

            promocaoDePascoa.IncluirProduto(p1);
            promocaoDePascoa.IncluirProduto(p2);
            promocaoDePascoa.IncluirProduto(p3);

            using (var contexto = new LojaContext())
            {
                var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(SqlLoggerProvider.Create());

                var promocao = contexto.Promocoes.Find(1);
                contexto.Promocoes.Remove(promocao);
                contexto.SaveChanges();
            }
        }

        private static void ExcluirCompras(int ComprasId)
        {
            using (var contexto = new LojaContext())
            {
                var compra = contexto.Compras.Where(c => c.Id == ComprasId).FirstOrDefault();
                
                var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(SqlLoggerProvider.Create());

                contexto.Compras.Remove(compra);
                contexto.SaveChanges();
            }
        }

        private static void ExcluirProdutos(int ProdutoId)
        {
            using (var contexto = new LojaContext())
            {
                var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(SqlLoggerProvider.Create());

                var produto = contexto.Produtos.Where(c => c.Id == ProdutoId).FirstOrDefault();

                contexto.Produtos.Remove(produto);
                contexto.SaveChanges();
            }
        }

        private static void AdicionaCompras(int ProdutoId, int Quantidade)
        {
            using (var contexto = new LojaContext())
            {
                var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(SqlLoggerProvider.Create());


                var produto = contexto.Produtos.Where(p => p.Id == ProdutoId).FirstOrDefault();
                var compra = new Compra()
                {
                    Quantidade = Quantidade,
                    Produto = produto,
                    Preco = produto.PrecoUnitario * Quantidade
                };

                contexto.Compras.Add(compra);
                contexto.SaveChanges();
            }
        }

        private static void ExibeEntries(IEnumerable<EntityEntry> entries)
        {
            foreach (var e in entries)
            {
                Console.WriteLine(e.Entity.ToString() + " - " + e.State);
            }
        }
    }
}