using FlashCard.Models;
using FlashCard.Services;
using System.Diagnostics;

namespace FlashCard.Pages;

public partial class QuizPage : ContentPage {
    private List<Card> _remainingCards;
    private Card _currentCard;
    private int _totalCards;
    private int _failedCount;
    private int _answeredCount;
    private Dictionary<Card, int> _cardFailures; //hashmap
    private Stopwatch _stopwatch;

    private Deck _deck;

    // about shake the phone
    private readonly ShakeDetectionService _shakeService;
    private int _shakeCount = 0;

    DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;

    public QuizPage(Deck deck, List<Card> cards) {
        InitializeComponent();
        _deck = deck;
        Title = $"Quiz - {deck.Name}";

        _totalCards = cards.Count;
        _failedCount = 0;
        _answeredCount = 0;
        _cardFailures = new Dictionary<Card, int>();
        _stopwatch = new Stopwatch();
        _stopwatch.Start();

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

    private async Task ShowNextQuestion() {
        if (_remainingCards.Count == 0) {
            ShowResults();
            return;
        }

        QuestionFrame.TranslationY = -mainDisplayInfo.Height / 2;

        _currentCard = _remainingCards[0];
        _remainingCards.RemoveAt(0);

        QuestionLabel.Text = _currentCard.Recto;
        UpdateProgress();

        QuestionFrame.IsVisible = true;
        AnswerFrame.IsVisible = false;
        ResultsFrame.IsVisible = false;


        await QuestionFrame.TranslateTo(QuestionFrame.X, 0, 800);

        _shakeService.StartMonitoring();

        AnswerFrame.TranslationY = 0;
    }

    private void UpdateProgress() {
        int totalToAnswer = _answeredCount + _remainingCards.Count + 1;
        ProgressLabel.Text = $"Question {_answeredCount + 1} / {totalToAnswer}";
        QuizProgressBar.Progress = (double)_answeredCount / Math.Max(totalToAnswer, 1);
    }

    private void OnKnowClicked(object sender, EventArgs e) {
        _answeredCount++;
        AnswerFrame.Background = new SolidColorBrush(Color.FromArgb("#95ff8a"));
        AnswerLabel.Text = _currentCard.Verso;
        OnShowAnswort(_currentCard.Verso);
    }

    private void OnDontKnowClicked(object sender, EventArgs e) {
        _shakeService.StopMonitoring();
        _failedCount++;
        _answeredCount++;

        if (_cardFailures.ContainsKey(_currentCard)) {
            _cardFailures[_currentCard]++;
        } else {
            _cardFailures[_currentCard] = 1;
        }

        AnswerLabel.Text = _currentCard.Verso;

        if (_remainingCards.Count > 0) {
            Random rnd = new Random();
            int insertIndex = rnd.Next(Math.Min(2, _remainingCards.Count), _remainingCards.Count + 1);
            _remainingCards.Insert(insertIndex, _currentCard);
        } else {
            _remainingCards.Add(_currentCard);
        }
        AnswerFrame.Background = new SolidColorBrush(Color.FromArgb("#ff8a8a"));
        OnShowAnswort(_currentCard.Verso);
    }

    private async void OnShowAnswort(string verso) {
        _shakeService.StopMonitoring();

        await QuestionFrame.RotateYTo(90, 500);

        QuestionFrame.IsVisible = false;
        AnswerFrame.IsVisible = true;
        ResultsFrame.IsVisible = false;

        AnswerFrame.RotationY = -90;
        await AnswerFrame.RotateYTo(0, 500);
        QuestionFrame.RotationY = 0;
    }

    private async void OnNextClicked(object sender, EventArgs e) {
        await AnswerFrame.TranslateTo(AnswerFrame.X, mainDisplayInfo.Height / 2 - 700, 500);
        ShowNextQuestion();
    }

    private void ShowResults() {
        _stopwatch.Stop();
        _shakeService.StopMonitoring();
        QuestionFrame.IsVisible = false;
        AnswerFrame.IsVisible = false;
        ResultsFrame.IsVisible = true;

        int correctCount = _totalCards - _failedCount;
        if (correctCount < 0) correctCount = 0;

        ScoreLabel.Text = $"Score : {correctCount} / {_totalCards}";
        FailedLabel.Text = $"Questions ratées : {_failedCount}";

        TimeLabel.Text = $"Temps passé : {_stopwatch.Elapsed.ToString(@"mm\:ss")}";

        int perfectCardsCount = _totalCards - _cardFailures.Count;
        PerfectCardsLabel.Text = $"Cartes sans faute : {perfectCardsCount}";

        double memorizationPercentage = _totalCards > 0 ? ((double)perfectCardsCount / _totalCards) * 100 : 0;
        MemorizationLabel.Text = $"Mémorisation : {Math.Round(memorizationPercentage)}%";

        if (_cardFailures.Count > 0) {
            var hardestCard = _cardFailures.OrderByDescending(x => x.Value).First().Key;
            HardestCardLabel.Text = $"Carte la plus difficile : {hardestCard.Recto}";
        } else {
            HardestCardLabel.Text = "Carte la plus difficile : Aucune !";
        }

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
