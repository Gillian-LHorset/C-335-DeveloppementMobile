using FlashCard.Models;
using FlashCard.Services;

namespace FlashCard.Pages;

public partial class QuizPage : ContentPage {
    private List<Card> _remainingCards;
    private Card _currentCard;
    private int _totalCards;
    private int _failedCount;
    private int _answeredCount;

    private Deck _deck;

    // about shake the phone
    private readonly ShakeDetectionService _shakeService;
    private int _shakeCount = 0;

    public QuizPage(Deck deck, List<Card> cards) {
        InitializeComponent();
        _deck = deck;
        Title = $"Quiz - {deck.Name}";

        _totalCards = cards.Count;
        _failedCount = 0;
        _answeredCount = 0;

        _remainingCards = new List<Card>(cards);
        ShuffleCards();

        _shakeService = new ShakeDetectionService();

        if (_shakeService.IsAvailable) {
            _shakeService.ShakeDetected += OnShakeDetected;
            _shakeService.StartMonitoring();
        }

        ShowNextQuestion();
    }

    /// <summary>
    /// randomize the cards
    /// </summary>
    private void ShuffleCards() {
        Random rnd = new Random();
        int i = _remainingCards.Count;
        while (i > 1) {
            i--;
            int j = rnd.Next(i + 1);
            Card tempCard = _remainingCards[j];
            _remainingCards[j] = _remainingCards[i];
            _remainingCards[i] = tempCard;
        }
    }

    private void ShowNextQuestion() {
        if (_remainingCards.Count == 0) {
            ShowResults();
            return;
        }

        _currentCard = _remainingCards[0];
        _remainingCards.RemoveAt(0);

        QuestionLabel.Text = _currentCard.Recto;
        UpdateProgress();

        QuestionFrame.IsVisible = true;
        AnswerFrame.IsVisible = false;
        ResultsFrame.IsVisible = false;
        _shakeService.StartMonitoring();
    }

    private void UpdateProgress() {
        int totalToAnswer = _answeredCount + _remainingCards.Count + 1;
        ProgressLabel.Text = $"Question {_answeredCount + 1} / {totalToAnswer}";
        QuizProgressBar.Progress = (double)_answeredCount / Math.Max(totalToAnswer, 1);
    }

    private void OnKnowClicked(object sender, EventArgs e) {
        _answeredCount++;
        ShowNextQuestion();
    }

    private void OnDontKnowClicked(object sender, EventArgs e) {
        _shakeService.StopMonitoring();
        _failedCount++;
        _answeredCount++;

        AnswerLabel.Text = _currentCard.Verso;

        if (_remainingCards.Count > 0) {
            Random rnd = new Random();
            int insertIndex = rnd.Next(Math.Min(2, _remainingCards.Count), _remainingCards.Count + 1);
            _remainingCards.Insert(insertIndex, _currentCard);
        } else {
            _remainingCards.Add(_currentCard);
        }

        QuestionFrame.IsVisible = false;
        AnswerFrame.IsVisible = true;
        ResultsFrame.IsVisible = false;

    }

    private void OnNextClicked(object sender, EventArgs e) {
        ShowNextQuestion();
    }

    private void ShowResults() {
        _shakeService.StopMonitoring();
        QuestionFrame.IsVisible = false;
        AnswerFrame.IsVisible = false;
        ResultsFrame.IsVisible = true;

        int correctCount = _totalCards - _failedCount;
        if (correctCount < 0) correctCount = 0;

        ScoreLabel.Text = $"Score : {correctCount} / {_totalCards}";
        FailedLabel.Text = $"Questions ratées : {_failedCount}";

        QuizProgressBar.Progress = 1;
        ProgressLabel.Text = "Terminé !";
    }

    private void OnShakeDetected(object sender, EventArgs e) {
        OnDontKnowClicked(sender, e);
    }

    private async void OnBackToDecksClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//DecksPage");
    }
}
