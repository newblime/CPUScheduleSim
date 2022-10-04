using Godot;
using System.Collections.Generic;

public class sched_d_compkd: IComparer<sched_scene.sched_d>{
  public int Compare(sched_scene.sched_d s1, sched_scene.sched_d s2){
    return s1.kedatangan.CompareTo(s2.kedatangan);
  }
}

public class sched_d_compid: IComparer<sched_scene.sched_d>{
  public int Compare(sched_scene.sched_d s1, sched_scene.sched_d s2){
    return s1.id.CompareTo(s2.id);
  }
}

public class sched_scene: ScrollContainer{
  [Export]
  private PackedScene colrect;

  public enum edit_id{
    durasi,
    kedatangan,
    prioritas
  }

  public struct sched_d{
    public int id, durasi, kedatangan, prioritas;
    public Color col;
  }


  private IEditInterface_Create.EditInterfaceContent[] sched_data = new IEditInterface_Create.EditInterfaceContent[]{
    new IEditInterface_Create.EditInterfaceContent{
      EditType = IEditInterface_Create.InterfaceType.Text,
      Properties = new EditText.set_param{
        strdata = "0",
        fullEdit = true
      },

      ID = (int)edit_id.durasi
    },

    new IEditInterface_Create.EditInterfaceContent{
      EditType = IEditInterface_Create.InterfaceType.Text,
      Properties = new EditText.set_param{
        strdata = "0",
        fullEdit = true
      },

      ID = (int)edit_id.kedatangan
    },

    new IEditInterface_Create.EditInterfaceContent{
      EditType = IEditInterface_Create.InterfaceType.Text,
      Properties = new EditText.set_param{
        strdata = "0",
        fullEdit = true
      },

      ID = (int)edit_id.prioritas
    },
  };

  private VBoxContainer _cont;
  private List<sched_d> sched_ds = new List<sched_d>();


  private void _sched_data_edit(int id, object obj){
    int idx = id >> 16;
    int ed_id = id & 0xFFFF;

    var _s = sched_ds[idx];
    switch((edit_id)ed_id){
      case edit_id.durasi:{
        EditText.get_data text = obj as EditText.get_data;
        if(text.strdata.Length <= 0)
          break;

        int _nparam = 0;
        int.TryParse(text.strdata, out _nparam);
        
        _s.durasi = _nparam;
        text.strdata = _nparam.ToString();
      }break;

      case edit_id.kedatangan:{
        EditText.get_data text = obj as EditText.get_data;
        if(text.strdata.Length <= 0)
          break;
          
        int _nparam = 0;
        int.TryParse(text.strdata, out _nparam);
        
        _s.kedatangan = _nparam;
        text.strdata = _nparam.ToString();
      }break;

      case edit_id.prioritas:{
        EditText.get_data text = obj as EditText.get_data;
        if(text.strdata.Length <= 0)
          break;
          
        int _nparam = 0;
        int.TryParse(text.strdata, out _nparam);
        
        _s.prioritas = _nparam;
        text.strdata = _nparam.ToString();
      }break;
    }

    sched_ds[idx] = _s;
  }


  public override void _Ready(){
    _cont = GetNode<VBoxContainer>("1");
  }

  public void AddSched(Color col){
    sched_ds.Add(new sched_d{col = col});

    HBoxContainer hb = new HBoxContainer();
    hb.Alignment = BoxContainer.AlignMode.Center;
    
    _cont.AddChild(hb);

    TextureRect tr = colrect.Instance<TextureRect>();
    tr.Modulate = col;
    
    Label l = tr.GetNode<Label>("Label");
    l.Text = "p" + sched_ds.Count;
    l.Modulate = col.Inverted();

    hb.AddChild(tr);

    for(int i = 0; i < sched_data.Length; i++){
      int id = sched_data[i].ID;
      id &= 0xFFFF;
      id |= ((sched_ds.Count-1) << 16);
      sched_data[i].ID = id;
    }

    IEditInterface_Create.Autoload.CreateAndAdd(hb, this, "_sched_data_edit", sched_data);
  }

  public void RemoveAllSched(){
    while(_cont.GetChildCount() > 0){
      var hb = _cont.GetChild(0);
      while(hb.GetChildCount() > 0){
        var _node = hb.GetChild(0);
        hb.RemoveChild(_node);

        _node.Dispose();
      }

      _cont.RemoveChild(hb);
      hb.Dispose();
    }

    sched_ds.Clear();
  }

  public List<sched_d> GetData(){
    return sched_ds;
  }
}