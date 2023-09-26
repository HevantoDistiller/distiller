
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Distiller.ViewModels.Production;

internal class RecipesViewModel : IQueryAttributable
{
    public ObservableCollection<RecipeViewModel> AllRecipes { get; }

    public RecipeViewModel CurrentRecipe { get; }
    public ICommand NewCommand { get; }
    public ICommand SelectRecipeCommand { get; }

    public RecipesViewModel()
    {
        AllRecipes = new ObservableCollection<RecipeViewModel>(
            Models.Recipe.LoadAllForCategory("Production")
                .Select(r => new RecipeViewModel(r)));
        CurrentRecipe = new RecipeViewModel();
        NewCommand = new AsyncRelayCommand(NewRecipeAsync);
        SelectRecipeCommand = new AsyncRelayCommand<RecipeViewModel>(SelectRecipeAsync);
    }

    private async Task NewRecipeAsync()
    {
        throw new NotImplementedException();
    }

    private async Task SelectRecipeAsync(RecipeViewModel recipe)
    {
        throw new NotImplementedException();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        throw new NotImplementedException();
    }
}