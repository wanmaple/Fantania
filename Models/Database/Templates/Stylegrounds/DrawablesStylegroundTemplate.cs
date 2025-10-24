using System;
using System.Collections.ObjectModel;

namespace Fantania.Models;

[DataGroup(Group = "StylegroundTemplates")]
public class DrawablesStylegroundTemplate : StylegroundTemplate
{
    public override Type StylegroundType => typeof(ScrollableStyleground);

    private int _seed = 0;
    [EditInteger(ControlType = typeof(RandomSeedControl))]
    public int Seed
    {
        get { return _seed; }
        set
        {
            if (_seed != value)
            {
                OnPropertyChanging(nameof(Seed));
                _seed = value;
                OnPropertyChanged(nameof(Seed));
            }
        }
    }
    
    private ObservableCollection<DrawTemplate> _drawables = new ObservableCollection<DrawTemplate>();
    [EditGroupReference("DrawTemplates")]
    public ObservableCollection<DrawTemplate> Drawables
    {
        get { return _drawables; }
        set
        {
            if (_drawables != value)
            {
                OnPropertyChanging(nameof(Drawables));
                _drawables = value;
                OnPropertyChanged(nameof(Drawables));
            }
        }
    }

    public override IRenderer CreateRenderer()
    {
        throw new NotImplementedException();
    }

    public override void UpdateRenderer(IRenderer renderer)
    {
        throw new NotImplementedException();
    }
}