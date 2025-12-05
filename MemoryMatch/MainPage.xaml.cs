using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MemoryMatch.ViewModels;
using MemoryMatch.Models;

namespace MemoryMatch;

public sealed partial class MainPage : Page
{
    private readonly GameViewModel _viewModel;
    private readonly Dictionary<int, Border> _cardElements = new();

    public MainPage()
    {
        this.InitializeComponent();
        _viewModel = new GameViewModel();
        
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Animate title entrance
        await AnimateEntrance(TitleText, 0);
        await AnimateEntrance(SubtitleText, 100);
        await AnimateMessageBanner();
        
        RefreshCardGrid();
        await AnimateCardsEntrance();
    }

    private async Task AnimateEntrance(FrameworkElement element, int delayMs)
    {
        await Task.Delay(delayMs);
        
        var storyboard = new Storyboard();
        
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(500)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeIn, element);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);
        
        storyboard.Begin();
    }

    private async Task AnimateMessageBanner()
    {
        await Task.Delay(300);
        
        var storyboard = new Storyboard();
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeIn, MessageBanner);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);
        storyboard.Begin();
    }

    private async Task AnimateCardsEntrance()
    {
        int index = 0;
        foreach (var kvp in _cardElements)
        {
            var element = kvp.Value;
            element.Opacity = 0;
            
            await Task.Delay(30);
            
            var storyboard = new Storyboard();
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            Storyboard.SetTarget(fadeIn, element);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");
            storyboard.Children.Add(fadeIn);
            storyboard.Begin();
            
            index++;
        }
    }

    private void RefreshCardGrid()
    {
        CardGrid.Items.Clear();
        _cardElements.Clear();

        foreach (var card in _viewModel.Cards)
        {
            var cardElement = CreateCardElement(card);
            CardGrid.Items.Add(cardElement);
            _cardElements[card.Id] = cardElement;
        }
    }

    private Border CreateCardElement(MemoryCard card)
    {
        // Card container with shadow
        var border = new Border
        {
            Width = 100,
            Height = 120,
            CornerRadius = new CornerRadius(16),
            Margin = new Thickness(5),
            Background = GetCardBackground(card),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Tag = card
        };

        var grid = new Grid();
        
        // Card content
        var contentText = new TextBlock
        {
            Text = card.IsFlipped || card.IsMatched ? card.Emoji : "❓",
            FontSize = card.IsFlipped || card.IsMatched ? 42 : 36,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Matched indicator overlay
        if (card.IsMatched)
        {
            var matchedOverlay = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(60, 67, 233, 123)),
                CornerRadius = new CornerRadius(16)
            };
            grid.Children.Add(matchedOverlay);
            
            // Add checkmark
            var checkmark = new TextBlock
            {
                Text = "✓",
                FontSize = 20,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 67, 233, 123)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 8, 10, 0)
            };
            grid.Children.Add(checkmark);
        }

        grid.Children.Add(contentText);
        border.Child = grid;

        // Add tap handler
        border.PointerPressed += async (s, e) => await OnCardTapped(card);
        border.PointerEntered += (s, e) => OnCardHoverEnter(border, card);
        border.PointerExited += (s, e) => OnCardHoverExit(border, card);

        return border;
    }

    private Brush GetCardBackground(MemoryCard card)
    {
        if (card.IsFlipped || card.IsMatched)
        {
            // Show colorful gradient when flipped
            return new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = ParseColor(card.GradientStart), Offset = 0 },
                    new GradientStop { Color = ParseColor(card.GradientEnd), Offset = 1 }
                }
            };
        }
        else
        {
            // Dark card back
            return new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Windows.UI.Color.FromArgb(255, 25, 25, 40), Offset = 0 },
                    new GradientStop { Color = Windows.UI.Color.FromArgb(255, 35, 35, 55), Offset = 1 }
                }
            };
        }
    }

    private Windows.UI.Color ParseColor(string hex)
    {
        hex = hex.Replace("#", "");
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return Windows.UI.Color.FromArgb(255, r, g, b);
    }

    private void OnCardHoverEnter(Border border, MemoryCard card)
    {
        if (!card.IsFlipped && !card.IsMatched)
        {
            border.Scale = new System.Numerics.Vector3(1.05f, 1.05f, 1);
        }
    }

    private void OnCardHoverExit(Border border, MemoryCard card)
    {
        border.Scale = new System.Numerics.Vector3(1, 1, 1);
    }

    private async Task OnCardTapped(MemoryCard card)
    {
        if (_cardElements.TryGetValue(card.Id, out var border))
        {
            // Quick scale animation
            var scaleDown = new System.Numerics.Vector3(0.95f, 0.95f, 1);
            border.Scale = scaleDown;
            await Task.Delay(50);
            border.Scale = new System.Numerics.Vector3(1, 1, 1);
        }

        await _viewModel.FlipCardCommand.ExecuteAsync(card);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(GameViewModel.Score):
                    ScoreText.Text = _viewModel.Score.ToString();
                    AnimateScoreChange();
                    break;
                    
                case nameof(GameViewModel.Moves):
                    MovesText.Text = _viewModel.Moves.ToString();
                    break;
                    
                case nameof(GameViewModel.MatchedPairs):
                    MatchesText.Text = $"{_viewModel.MatchedPairs}/8";
                    break;
                    
                case nameof(GameViewModel.Combo):
                    ComboText.Text = $"{_viewModel.Combo}x";
                    if (_viewModel.Combo > 1)
                    {
                        AnimateCombo();
                    }
                    break;
                    
                case nameof(GameViewModel.Message):
                    MessageText.Text = _viewModel.Message;
                    AnimateMessage();
                    break;
                    
                case nameof(GameViewModel.BestScore):
                    BestScoreText.Text = _viewModel.BestScore.ToString();
                    break;
                    
                case nameof(GameViewModel.IsGameComplete):
                    if (_viewModel.IsGameComplete)
                    {
                        ShowWinOverlay();
                    }
                    break;
                    
                case nameof(GameViewModel.Cards):
                    RefreshCards();
                    break;
            }
        });
    }

    private void RefreshCards()
    {
        foreach (var card in _viewModel.Cards)
        {
            if (_cardElements.TryGetValue(card.Id, out var border))
            {
                var newCard = CreateCardElement(card);
                var index = CardGrid.Items.IndexOf(border);
                if (index >= 0)
                {
                    CardGrid.Items[index] = newCard;
                    _cardElements[card.Id] = newCard;
                }
            }
        }
    }

    private void AnimateScoreChange()
    {
        var storyboard = new Storyboard();
        
        var scaleUp = new DoubleAnimation
        {
            From = 1.0,
            To = 1.3,
            Duration = new Duration(TimeSpan.FromMilliseconds(100)),
            AutoReverse = true
        };
        
        Storyboard.SetTarget(scaleUp, ScoreText);
        Storyboard.SetTargetProperty(scaleUp, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        
        ScoreText.RenderTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        ScoreText.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
    }

    private void AnimateCombo()
    {
        ComboText.Opacity = 0;
        var storyboard = new Storyboard();
        
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };
        
        Storyboard.SetTarget(fadeIn, ComboText);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);
        storyboard.Begin();
    }

    private void AnimateMessage()
    {
        var storyboard = new Storyboard();
        
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0.7,
            Duration = new Duration(TimeSpan.FromMilliseconds(100)),
            AutoReverse = true
        };
        
        Storyboard.SetTarget(fadeOut, MessageBanner);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");
        storyboard.Children.Add(fadeOut);
        storyboard.Begin();
    }

    private void ShowWinOverlay()
    {
        WinScoreText.Text = $"Final Score: {_viewModel.Score}";
        WinMovesText.Text = $"Moves: {_viewModel.Moves}";
        WinOverlay.Visibility = Visibility.Visible;
        
        var storyboard = new Storyboard();
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeIn, WinOverlay);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);
        storyboard.Begin();
    }

    private async void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        WinOverlay.Visibility = Visibility.Collapsed;
        _viewModel.InitializeGameCommand.Execute(null);
        RefreshCardGrid();
        await AnimateCardsEntrance();
    }
}
