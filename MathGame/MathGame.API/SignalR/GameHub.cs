using MathGame.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MathGame.API.SignalR;

public class GameHub : Hub
{
    private readonly IGameService _gameService;

    public GameHub(IGameService gameService) => _gameService = gameService;

    public override async Task OnConnectedAsync()
    {
        var question = _gameService.GenerateQuestion();
        await Clients.Caller.SendAsync("NewQuestion", new
        {
            question.Expression,
            CorrectAnswer = question.Answer
        });
    }

    public async Task SubmitAnswer(string username, double correctAnswer, double proposedAnswer, bool isCorrect)
    {
        try
        {
            var isCorrectAnswer = Math.Abs(proposedAnswer - correctAnswer) < 0.001;

            var result = new
            {
                Username = username,
                IsCorrect = isCorrectAnswer,
                UserAnswer = proposedAnswer.ToString(),
                ExpectedAnswer = correctAnswer.ToString("0.###")
            };

            await Clients.All.SendAsync("ReceiveResult", result);

            var question = _gameService.GenerateQuestion();
            await Clients.All.SendAsync("NewQuestion", new
            {
                question.Expression,
                CorrectAnswer = question.Answer
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SubmitAnswer: {ex.Message}");
            throw;
        }
    }

}
