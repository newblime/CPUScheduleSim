using Godot;
using System;
using System.Collections.Generic;

public class main_scene : Control{
  [Export]
  private int max_schedule;

  [Export]
  private int max_hduration;

  [Export]
  private float _ratio = 0.15f;

  public enum _editid{
    tipesched,
    qt
  }

  public struct _sched_part{
    public int id;
    public Color col;
    public int low, high;
  }

  private TextureRect rectSched;
  private Control blockMouse;
  private Label timeLabel, logLabel;
  private sched_scene ss;

  private RandomNumberGenerator _rng = new RandomNumberGenerator();

  private List<int> ata_list = new List<int>();

  private List<sched_scene.sched_d> _data = new List<sched_scene.sched_d>();
  private List<_sched_part> _partdata = new List<_sched_part>();

  private int _tipeSched = 0;
  private int _qtn = 0;

  private float _time;
  private int _datadone;
  private int _hduration;
  private int _timekd;
  private int _partidx;
  private bool _simulating = false;
  private bool _updateOnce = true;

  private IComparer<sched_scene.sched_d> _schedcomp_kd = new sched_d_compkd(), _schedcomp_id = new sched_d_compid();

  private void _tambah_pressed(){
    if(ss.GetData().Count < max_schedule){
      Color _randcol = Color.Color8((byte)_rng.RandiRange(0,0xff), (byte)_rng.RandiRange(0,0xff), (byte)_rng.RandiRange(0,0xff));

      ss.AddSched(_randcol);
    }
  }

  private void _simulasi_pressed(){
    if(ss.GetData().Count > 0){
      _data.Clear();
      var _schedlist = ss.GetData();
      for(int i = 0; i < _schedlist.Count; i++){
        if(_schedlist[i].durasi > 0){
          _data.Add(_schedlist[i]);
          
          if(_schedlist[i].durasi > _hduration)
            _hduration = _schedlist[i].durasi;
        }
      }

      if(_data.Count <= 0){
        _hapus_pressed();
        return;
      }

      ata_list.Clear();
      for(int i = 0; i < _data.Count; i++)
        ata_list.Add(0);

      _data.Sort(_schedcomp_kd);
      _timekd = _data[0].kedatangan;

      _time = 0.0f;
      _proses_nextsched();

      var shad = rectSched.Material as ShaderMaterial;

      _hduration = Mathf.Min(_hduration, max_hduration);
      shad.SetShaderParam("hduration", _hduration);

      _simulating = true;
      _datadone = 0;
      _partidx = 0;
      _hduration = 0;
      blockMouse.Visible = true;
    }
  }

  private void _hapus_pressed(){
    if(ss.GetData().Count > 0){
      ss.RemoveAllSched();
      _data.Clear();
      _partdata.Clear();
    }
  }

  private void _change_ed(int id, object obj){
    switch((_editid)id){
      case _editid.tipesched:{
        var ed = obj as EditChoice.get_data;
        _tipeSched = ed.choiceID;
      }break;

      case _editid.qt:{
        var ed = obj as EditText.get_data;
        int.TryParse(ed.strdata, out _qtn);
        ed.strdata = _qtn.ToString();
      }break;
    }
  }

  private void _add_scheddata(SchedulingAlg.data_schedule s, int time){
    GD.Print(time);

    if(time > 0){
      _sched_part part = new _sched_part();
      part.id = s.id;
      part.col = _data[s.id].col;
      part.low = _partdata.Count > 0? _partdata[_partdata.Count-1].high: 0;
      part.low = _time > part.low? Mathf.RoundToInt(_time): part.low;
      part.high = part.low+time;
 
      // ata
      ata_list[s.id] = part.high;

      _partdata.Add(part);
    }
  }

  private void _update_schedpart(){
    Image impc = new Image(), impr = new Image();
    impc.Create(_partdata.Count, 1, false, Image.Format.Rgb8); 
    impr.Create(_partdata.Count, 1, false, Image.Format.Rgf);
    impc.Lock();
    impr.Lock();

    for(int i = 0; i < _partdata.Count; i++){
      var _p = _partdata[i];
      impc.SetPixel(i, 0, _p.col);

      GD.Print(_p.col.ToHtml());
      GD.Print(_p.low, " ", _p.high);

      Color cr = new Color();
      cr.r = (float)_p.low;
      cr.g = (float)_p.high;
      impr.SetPixel(i, 0, cr);
    }

    List<bool> awt_list = new List<bool>();

    int _len = 0;
    float ata_res = 0;
    for(int i = 0; i < _data.Count; i++){
      if(ata_list[i] > 0){
        ata_res += ata_list[i];
        _len++;
      }

      awt_list.Add(false);
    }

    ata_res /= _len;

    float awt_res = 0;
    for(int i = _partdata.Count-1; i >= 0; i--){
      awt_list[_partdata[i].id] = true;
      int dur = _partdata[i].high - _partdata[i].low;
      for(int o = 0; o < awt_list.Count; o++){
        if(awt_list[o] && o != _partdata[i].id)
          awt_res += dur;
      }
    }

    awt_res /= _len;

    logLabel.Text = string.Format("Average Waiting Time {0}, Average Turn Around {1}", awt_res.ToString("0.0"), ata_res.ToString("0.0"));

    impc.Unlock();
    impr.Unlock();

    ImageTexture texpc = new ImageTexture(), texpr = new ImageTexture();
    texpc.CreateFromImage(impc, (int)Texture.FlagsEnum.MirroredRepeat);
    texpr.CreateFromImage(impr, (int)Texture.FlagsEnum.MirroredRepeat);

    var shad = rectSched.Material as ShaderMaterial;
    shad.SetShaderParam("pcol", texpc);
    shad.SetShaderParam("prange", texpr);
    shad.SetShaderParam("plen", _partdata.Count);
  }

  private void _reduce_t_data(int idx, int time){
    var _sd = _data[idx];
    _sd.durasi -= time;

    if(_sd.durasi <= 0){
      _datadone++;
      if(_datadone >= _data.Count){
        // then the simulation has finished
        _simulating = false;
        _hapus_pressed();
        blockMouse.Visible = false;
        return;
      }
    }
    
    _data[idx] = _sd;
  }

  private void _proses_nextsched(){
    int idx = -1;
    for(int i = 0; i < _data.Count; i++){
      if(_data[i].kedatangan == _timekd && idx == -1)
        idx = i;

      sched_scene.sched_d d = _data[i];
      d.id = i;

      _data[i] = d;
    }

    if(idx < 0){
      return;
    }

    List<SchedulingAlg.data_schedule> sched = new List<SchedulingAlg.data_schedule>();
    
    // if preemptive
    if(_Sched.objs[_tipeSched].Preemptive && _partdata.Count > 0){
      // reduce time
      int _pidx = _partdata[_partidx].id;
      int _delta = _timekd-_partdata[_partidx].low;
      _reduce_t_data(_pidx, _delta);
      if(!_simulating)
        return;

      // then remove some part data ahead
      var _pd = _partdata[_partidx];
      _pd.high = _timekd;
      _partdata[_partidx] = _pd;

      _partdata.RemoveRange(_partidx+1, _partdata.Count-_partidx-1);

      // adding some data that still has duration
      for(int i = 0; i < idx; i++){
        if(_data[i].durasi > 0){
          SchedulingAlg.data_schedule ds = new SchedulingAlg.data_schedule{
            id = _data[i].id,
            waktu = _data[i].durasi,
            prioritas = _data[i].kedatangan
          };

          sched.Add(ds);
        }
      }
    }

    while(idx < _data.Count && _data[idx].kedatangan == _timekd){
      SchedulingAlg.data_schedule ds = new SchedulingAlg.data_schedule{
        id = _data[idx].id,
        waktu = _data[idx].durasi,
        prioritas = _data[idx].kedatangan
      };

      sched.Add(ds);
      idx++;
    }
    
    _Sched.objs[_tipeSched].proses(sched.ToArray(), sched.Count, _qtn);

    if(idx < _data.Count)
      _timekd = _data[idx].kedatangan;
    else
      _timekd = _partdata[_partdata.Count-1].high; // assuming that the scheduling already calculated
    
    GD.Print("time ", _timekd);

    _update_schedpart();
  }


  public override void _Ready(){
    _rng.Randomize();
    _Sched.f = _add_scheddata;

    rectSched = GetNode<TextureRect>("2/3");
    blockMouse = GetNode<Control>("Block");
    timeLabel = GetNode<Label>("1");
    logLabel = GetNode<Label>("2/log_label");
    ss = GetNode<sched_scene>("2/2");

    blockMouse.Visible = false;
    timeLabel.Text = "-ms";

    GetNode<Button>("2/1/1").Connect("pressed", this, "_tambah_pressed");
    GetNode<Button>("2/1/2").Connect("pressed", this, "_simulasi_pressed");
    GetNode<Button>("2/1/3").Connect("pressed", this, "_hapus_pressed");

    KeyValuePair<string, int>[] algs = new KeyValuePair<string, int>[_Sched.objs.Length];
    for(int i = 0; i < algs.Length; i++)
      algs[i] = new KeyValuePair<string, int>(_Sched.objs[i].AlgName, i);

    IEditInterface_Create.EditInterfaceContent[] _editcont = new IEditInterface_Create.EditInterfaceContent[]{
      new IEditInterface_Create.EditInterfaceContent{
        TitleName = "Tipe Scheduling",
        EditType = IEditInterface_Create.InterfaceType.Choice,
        Properties = new EditChoice.set_param{
          choices = algs
        },

        ID = (int)_editid.tipesched
      },

      new IEditInterface_Create.EditInterfaceContent{
        TitleName = "Quantum Time",
        EditType = IEditInterface_Create.InterfaceType.Text,
        Properties = new EditText.set_param{
          strdata = "0"
        },

        ID = (int)_editid.qt
      }
    };

    IEditInterface_Create.Autoload.CreateAndAdd(GetNode("2"), this, "_change_ed", _editcont);

    var shad = rectSched.Material as ShaderMaterial;
    shad.SetShaderParam("_ratio", _ratio);
  }

  public override void _Process(float delta){
    if(_updateOnce){
      _updateOnce = false;
      timeLabel.RectPosition = new Vector2(rectSched.RectSize.x*_ratio, rectSched.RectPosition.y-timeLabel.RectSize.y);
    }

    if(_simulating){
      _time += delta;
      int timeint = Mathf.FloorToInt(_time);

      timeLabel.Text = string.Format("{0}ms | p{1}", _time.ToString("0.00"), _partidx >= _partdata.Count? (object)"-": (object)(_partdata[_partidx].id+1));

      var shad = rectSched.Material as ShaderMaterial;
      shad.SetShaderParam("_time", _time);
      
      while(_data.Count > 0 && _partidx < _partdata.Count && timeint >= _partdata[_partidx].high){
        int _pidx = _partdata[_partidx].id;

        if(_pidx < 0){
          GD.Print("err id: ", _partdata[_partidx].id, " ", _timekd);
          foreach(var d in _data){
            GD.Print(d.id);
          }
          break;
        }

        _reduce_t_data(_pidx, _partdata[_partidx].high - _partdata[_partidx].low);
        if(!_simulating)
          return;

        _partidx++;
      }
      
      if(timeint >= _timekd){
        _proses_nextsched();
      }
    }
  }
}
