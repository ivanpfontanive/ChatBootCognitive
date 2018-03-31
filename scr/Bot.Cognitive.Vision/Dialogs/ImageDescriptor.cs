using Bot.Cognitive.Vision.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Cognitive.Vision.Dialogs
{
    [Serializable]
    public class ImageDescriptor : LuisDialog<object>
    {
        private enum TipoDeProcessamento { Emocoes, Descricao, Classificacao }

        public ImageDescriptor(ILuisService service) : base(service)
        {
        }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, eu não entendi...\n" +
                                    "Lembre-se que sou um bot e meu conhecimento é limitado.");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando não houve intenção reconhecida.
        /// </summary>
        [LuisIntent("")]
        public async Task IntencaoNaoReconhecida(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, mas não entendi o que você quis dizer.\n" +
                                    "Lembre-se que sou um bot e meu conhecimento é limitado.");
            context.Done<string>(null);
        }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("consciencia")]
        public async Task ConscienciaAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sou um bot que classifica imagens!");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando a intenção for uma saudação.
        /// </summary>
        [LuisIntent("saudar")]
        public async Task Saudar(IDialogContext context, LuisResult result)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).TimeOfDay;
            string saudacao;

            if (now < TimeSpan.FromHours(12)) saudacao = "Bom dia";
            else if (now < TimeSpan.FromHours(18)) saudacao = "Boa tarde";
            else saudacao = "Boa noite";

            await context.PostAsync($"{saudacao}! Em que posso ajudar?");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando a intenção for descrever uma imagem.
        /// </summary>
        [LuisIntent("descrever-imagem")]
        public async Task DescreverImagen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Me envia uma imagem que eu descrevo o que tem nela.");
            context.Wait((c, a) => ProcessarImagemAsync(c, a, TipoDeProcessamento.Descricao));
        }

        /// <summary>
        /// Quando a intenção for classificar uma imagem.
        /// </summary>
        [LuisIntent("classificar-imagem")]
        public async Task ClassificarImagem(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Me envia uma imagem e eu te direi o que ela é!");
            context.Wait((c, a) => ProcessarImagemAsync(c, a, TipoDeProcessamento.Classificacao));
        }

        private async Task ProcessarImagemAsync(IDialogContext contexto,
            IAwaitable<IMessageActivity> argument,
            TipoDeProcessamento tipoDeProcessamento)
        {
            var activity = await argument;

            var uri = activity.Attachments?.Any() == true ?
                new Uri(activity.Attachments[0].ContentUrl) :
                new Uri(activity.Text);

            try
            {
                string reply;

                switch (tipoDeProcessamento)
                {
                    case TipoDeProcessamento.Descricao:
                        reply = await new VisaoComputacional().AnaliseDetalhadaAsync(uri);
                        break;

                    case TipoDeProcessamento.Classificacao:
                        reply = await new VisaoComputacional().ClassificacaoCustomizadaAsync(uri);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoDeProcessamento),
                            tipoDeProcessamento, null);
                }
                await contexto.PostAsync(reply);
            }
            catch (Exception)
            {
                await contexto.PostAsync("Ops! Deu algo errado na hora de analisar sua imagem!");
            }

            contexto.Wait(MessageReceived);
        }
    }
}