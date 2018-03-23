using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Cognitive.QnaMaker.Dialogs
{
    [Serializable]
    public class QnaDialog : QnAMakerDialog
    {
        public QnaDialog()
            : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnaSubscriptionKey"], ConfigurationManager.AppSettings["QnaKnowledgeBaseId"], "Não sei a resposta!", 0.5)))
        {
        }

        protected override Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            var activity = (context.Activity as Activity).CreateReply();

            var heroCard = new HeroCard();
            heroCard.Title = activity.Text;
            heroCard.Text = result.Answers.First().Answer;

            activity.Attachments.Add(heroCard.ToAttachment());
            return context.PostAsync(activity);
        }
    }
}