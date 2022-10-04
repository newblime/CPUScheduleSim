

struct data_schedule{
  int waktu;
  int prioritas;
};

// untuk menampilkan scheduling
void tampilkan_schedule(data_schedule s, int waktu);


// pada fungsi ini, parameter sched bakal diurutkan berdasarkan urutan dan waktu kedatangan
// qt, quantum time
void proses(data_schedule *sched, int sched_len, int qt){

  // contoh FIFO (first in, first out)
  
  // karena array sched memang sudah di urutkan, maka langsung aja dari pertama sampai akhir
  for(int i = 0; i < sched_len; i++){

    // kemudian tampilkan schedulenya
    tampilkan_schedule(sched[i], sched[i].waktu);
  }

  bool looping = true;
  while(looping){
    looping = false;
  }
}