using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryMatch.Models;

namespace MemoryMatch.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private readonly string[] _emojis = { "🚀", "🎮", "🌟", "🔥", "💎", "🎯", "🌈", "⚡" };
    
    private readonly (string start, string end)[] _gradients = 
    {
        ("#667eea", "#764ba2"),  // Purple
        ("#f093fb", "#f5576c"),  // Pink
        ("#4facfe", "#00f2fe"),  // Cyan
        ("#43e97b", "#38f9d7"),  // Green
        ("#fa709a", "#fee140"),  // Sunset
        ("#a8edea", "#fed6e3"),  // Soft
        ("#ff0844", "#ffb199"),  // Coral
        ("#5ee7df", "#b490ca"),  // Cool
    };

    [ObservableProperty]
    private ObservableCollection<MemoryCard> _cards = new();

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _moves;

    [ObservableProperty]
    private int _matchedPairs;

    [ObservableProperty]
    private bool _isGameComplete;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _message = "🎮 Match the pairs!";

    [ObservableProperty]
    private int _combo;

    [ObservableProperty]
    private int _bestScore;

    private MemoryCard? _firstFlippedCard;
    private MemoryCard? _secondFlippedCard;

    public GameViewModel()
    {
        InitializeGame();
    }

    [RelayCommand]
    public void InitializeGame()
    {
        Cards.Clear();
        Score = 0;
        Moves = 0;
        MatchedPairs = 0;
        Combo = 0;
        IsGameComplete = false;
        IsProcessing = false;
        Message = "🎮 Match the pairs!";
        _firstFlippedCard = null;
        _secondFlippedCard = null;

        var cardList = new List<MemoryCard>();
        int cardId = 0;

        for (int i = 0; i < _emojis.Length; i++)
        {
            var gradient = _gradients[i];
            
            // Create pair of cards
            cardList.Add(new MemoryCard 
            { 
                Id = cardId++, 
                Emoji = _emojis[i], 
                PairId = i,
                GradientStart = gradient.start,
                GradientEnd = gradient.end
            });
            
            cardList.Add(new MemoryCard 
            { 
                Id = cardId++, 
                Emoji = _emojis[i], 
                PairId = i,
                GradientStart = gradient.start,
                GradientEnd = gradient.end
            });
        }

        // Shuffle cards using Fisher-Yates
        var random = new Random();
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
        }

        foreach (var card in cardList)
        {
            Cards.Add(card);
        }
    }

    [RelayCommand]
    public async Task FlipCard(MemoryCard card)
    {
        if (IsProcessing || card.IsFlipped || card.IsMatched)
            return;

        card.IsFlipped = true;
        RefreshCard(card);

        if (_firstFlippedCard == null)
        {
            _firstFlippedCard = card;
            Message = "🤔 Pick another card...";
        }
        else if (_secondFlippedCard == null)
        {
            _secondFlippedCard = card;
            Moves++;
            IsProcessing = true;

            await CheckForMatch();
        }
    }

    private async Task CheckForMatch()
    {
        if (_firstFlippedCard == null || _secondFlippedCard == null)
            return;

        await Task.Delay(600); // Let player see the cards

        if (_firstFlippedCard.PairId == _secondFlippedCard.PairId)
        {
            // Match found!
            _firstFlippedCard.IsMatched = true;
            _secondFlippedCard.IsMatched = true;
            MatchedPairs++;
            Combo++;
            
            int points = 100 * Combo; // Combo multiplier!
            Score += points;
            
            if (Score > BestScore)
                BestScore = Score;

            Message = Combo > 1 
                ? $"🔥 {Combo}x COMBO! +{points} points!" 
                : "✨ Perfect match! +100 points!";

            RefreshCard(_firstFlippedCard);
            RefreshCard(_secondFlippedCard);

            if (MatchedPairs == _emojis.Length)
            {
                IsGameComplete = true;
                Message = $"🎉 YOU WIN! Score: {Score} | Moves: {Moves}";
            }
        }
        else
        {
            // No match
            Combo = 0;
            Message = "❌ No match! Try again...";
            
            await Task.Delay(200);
            
            _firstFlippedCard.IsFlipped = false;
            _secondFlippedCard.IsFlipped = false;
            
            RefreshCard(_firstFlippedCard);
            RefreshCard(_secondFlippedCard);
        }

        _firstFlippedCard = null;
        _secondFlippedCard = null;
        IsProcessing = false;
    }

    private void RefreshCard(MemoryCard card)
    {
        var index = Cards.IndexOf(card);
        if (index >= 0)
        {
            Cards[index] = card;
            OnPropertyChanged(nameof(Cards));
        }
    }
}
