using CommunityToolkit.Mvvm.ComponentModel;

namespace RPRO.App.ViewModels;

public class TestViewModel : ObservableObject
{
    public TestViewModel()
    {
        App.LogError("TestViewModel criado");
    }
}
