using System.Collections.Generic;


public class FIFO_ALG : SchedulingAlg
{
    public FIFO_ALG()
    {
        AlgName = "FIFO";
    }

    // pada fungsi ini, parameter sched bakal diurutkan berdasarkan urutan dan waktu kedatangan
    // qt, quantum time
    public override void proses(data_schedule[] sched, int sched_len, int qt)
    {

        // contoh FIFO (first in, first out)

        // karena array sched memang sudah di urutkan, maka langsung aja dari pertama sampai akhir
        for (int i = 0; i < sched_len; i++)
        {

            // kemudian tampilkan schedulenya
            tampilkan_schedule(sched[i], sched[i].waktu);
        }
    }
}

public class shortest_ALG : SchedulingAlg
{
    public class ds_comp_dur : IComparer<data_schedule>
    {
        public int Compare(data_schedule d1, data_schedule d2)
        {
            return d1.waktu.CompareTo(d2.waktu);
        }
    }

    public shortest_ALG()
    {
        AlgName = "Shortest Duration";
    }

    public override void proses(data_schedule[] sched, int sched_len, int qt)
    {
        List<data_schedule> dslist = new List<data_schedule>();
        for (int i = 0; i < sched_len; i++)
            dslist.Add(sched[i]);

        var sorter_comp = new ds_comp_dur();
        dslist.Sort(sorter_comp);
        for (int i = 0; i < sched_len; i++)
            tampilkan_schedule(dslist[i], dslist[i].waktu);
    }
}
public class round_robin_ALG : SchedulingAlg
{
    public override void proses(data_schedule[] sched, int sched_len, int qt)
    {
        while (true)
        {
            int end = 0;
            for (int i = 0; i < sched_len; i++)
            {
                if (sched[i].waktu > qt)
                {
                    sched[i].waktu -= qt;
                    tampilkan_schedule(sched[i], qt);
                }
                else if (sched[i].waktu <= qt && sched[i].waktu > 0)
                {
                    tampilkan_schedule(sched[i], sched[i].waktu);
                    sched[i].waktu -= qt;
                }
                else
                {
                    end++;
                }
            }
            if (end >= sched_len)
            {
                break;
            }
        }
    }
}