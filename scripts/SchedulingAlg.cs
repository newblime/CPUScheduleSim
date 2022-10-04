
public class _Sched{
  public delegate void _ts(SchedulingAlg.data_schedule s, int w);

  public static _ts f;
  public static SchedulingAlg[] objs = {
    new FIFO_ALG(),
    new shortest_ALG(),
    new shortest_ALG{AlgName = "Shortest Duration (Preemptive)", Preemptive = true}
  };
}

public class SchedulingAlg{
  public string AlgName;
  public bool Preemptive = false;

  public struct data_schedule{
    public int id;
    public int waktu;
    public int prioritas;
  }

  protected void tampilkan_schedule(data_schedule s, int waktu){
    _Sched.f(s, waktu);
  }
  
  public virtual void proses(data_schedule[] sched, int sched_len, int qt){}
}