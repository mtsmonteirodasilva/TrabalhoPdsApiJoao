using atividadeapi;

namespace atividadeapi.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class FornecedorController : ControllerBase
    {
        private const string Arquivo = "Dados Fornecedor.txt";

        [HttpGet]
        public IActionResult Get()
        {
            var fornecedores = LerArquivo();
            return Ok(fornecedores);
        }
        private bool ValidarCNPJ(string cnpj)
        {
            cnpj = Regex.Replace(cnpj, @"\D", "");
            return cnpj.Length == 14;
        }

        private void GravarArquivo(List<Fornecedores> fornecedores)
        {
            var linhas = fornecedores.Select(f => $"{f.NomeFantasia}|" +
            $"{f.RazaoSocial}|" + $"{f.CNPJ}|" + $"{f.Endereco}|" + $"{f.Cidade}|" + $"{f.Estado}|" + $"{f.Telefone}|" + $"{f.Email}|" + $"{f.Responsavel}");

            System.IO.File.WriteAllLines(Arquivo, linhas);
        }
        
        private List<Fornecedores> LerArquivo()
        {
            var fornecedores = new List<Fornecedores>();

            if (!System.IO.File.Exists(Arquivo))
            {
                return fornecedores;
            }

            var linhas = System.IO.File.ReadAllLines(Arquivo);
            foreach (var linha in linhas)
            {
                var dados = linha.Split('|');
                fornecedores.Add(new Fornecedores
                {
                    NomeFantasia = dados[0],
                    RazaoSocial = dados[1],
                    CNPJ = dados[2],
                    Endereco = dados[3],
                    Cidade = dados[4],
                    Estado = dados[5],
                    Telefone = dados[6],
                    Email = dados[7],
                    Responsavel = dados[8]
                });
            }

            return fornecedores;
        }


        [HttpGet("{cnpj}")]
        public IActionResult GetByCNPJ(string cnpj)
        {
            if (!ValidarCNPJ(cnpj))
            {
                return BadRequest("CNPJ inválido.");
            }

            var fornecedores = LerArquivo();
            var fornecedor = fornecedores.FirstOrDefault(f => f.CNPJ == cnpj);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return Ok(fornecedor);
        }

        [HttpPost]
        public IActionResult Post([FromBody] FornecedorDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Dados inválidos.");
            }

            var cnpj = dto.CNPJ;
            if (!ValidarCNPJ(cnpj))
            {
                return BadRequest("CNPJ inválido.");
            }

            var fornecedores = LerArquivo();
            if (fornecedores.Any(f => f.CNPJ == cnpj))
            {
                return Conflict("Fornecedor existente.");
            }

            var fornecedor = new Fornecedores
            {
                NomeFantasia = dto.NomeFantasia,
                RazaoSocial = dto.RazaoSocial,
                CNPJ = cnpj,
                Endereco = dto.Endereco,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Telefone = dto.Telefone,
                Email = dto.Email,
                Responsavel = dto.Responsavel
            };

            fornecedores.Add(fornecedor);
            GravarArquivo(fornecedores);

            return CreatedAtAction(nameof(GetByCNPJ), new { cnpj = fornecedor.CNPJ }, fornecedor);
        }

        [HttpPut("{cnpj}")]
        public IActionResult Put(string cnpj, [FromBody] FornecedorDTO dto)
        {
            if (!ValidarCNPJ(cnpj))
            {
                return BadRequest("CNPJ inválido.");
            }

            var fornecedores = LerArquivo();
            var fornecedor = fornecedores.FirstOrDefault(f => f.CNPJ == cnpj);

            if (fornecedor == null)
            {
                return NotFound();
            }

            fornecedor.NomeFantasia = dto.NomeFantasia ?? fornecedor.NomeFantasia;
            fornecedor.RazaoSocial = dto.RazaoSocial ?? fornecedor.RazaoSocial;
            fornecedor.Endereco = dto.Endereco ?? fornecedor.Endereco;
            fornecedor.Cidade = dto.Cidade ?? fornecedor.Cidade;
            fornecedor.Estado = dto.Estado ?? fornecedor.Estado;
            fornecedor.Telefone = dto.Telefone ?? fornecedor.Telefone;
            fornecedor.Email = dto.Email ?? fornecedor.Email;
            fornecedor.Responsavel = dto.Responsavel ?? fornecedor.Responsavel;

            GravarArquivo(fornecedores);

            return Ok(fornecedor);
        }

        [HttpDelete("{cnpj}")]
        public IActionResult Deletar(string cnpj)
        {
            if (!ValidarCNPJ(cnpj))
            {
                return BadRequest("CNPJ inválido. Tente novamente!");
            }

            var fornecedores = LerArquivo();
            var fornecedor = fornecedores.FirstOrDefault(f => f.CNPJ == cnpj);

            if (fornecedor == null)
            {
                return NotFound();
            }

            fornecedores.Remove(fornecedor);
            GravarArquivo(fornecedores);

            return Ok(fornecedor);
        }
    }
}
