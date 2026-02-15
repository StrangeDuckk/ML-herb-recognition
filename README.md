# ML-herb-recognition
# ML-rozpoznawanie-ziół
Mashine Learning Herb Recognition


plan dzialania i oczekiwania:
chcce stworzyc strone (stojaca samodzielnie ale napisana w pelni przeze mnie) do rozpoznawania zioł, chce moc w tle dac zdjecie ziola i fragment bazy, najpopularniesze wyszukiwanie z filtrami po polach z bazy i w miejscach wybranych przeze mnie opcje wyboru (interakcja z uzytkownikiem): rozpoznanie albo dodanie do bazy

- rozpoznanie: oddzielna strona, w tle obrazek wstawiony z zewnatrz i w odpowiednich miejscach pola do wyboru opcji z dostepnych + dodatkowa do wpisania

- dodanie do bazy: formularz dodajacy rosline do zaakceptowania po stronie administratora (mnie), mozliwosc wypelnienia wszystkich pol jak np srednia wysokosc, czy kwitnie (jak tak to jaki kolor, ksztalt kwiatow, wielkosc itd), kolor lisci, ksztalt lisci, lodyka ksztalt, czy jest kolące i dodatkowe informacje

chce zeby strona dzialala na podstawie uczenia maszynowego, klasyfikacji do konkretnej rosliny i wyprowadzala wyniki z 5 najpewniejszych wynikow przy opcji wyszukania ziola
chce tez aby przy kazdej roslinie byla mozliwosc dopisania np przepisu i dodania zdjec oraz podpiecia zdjec z google

pytania:
w jakich technologiach powinnam to pisac -> 
czego uzywac zeby bylo najmniej awaryjne, 
jakiej bazy uzyc, 
gdzie postawic serwer bazy i strone aby dzialala 24/7 i bylo najtaniej
jakiego modelu uzyc

projekt do CV i do uzytku prywatnego na posade zwiazana z uczeniem maszynowym i modelami uczenia
docelowo chcialabym zeby mozna bylo sciagnac baze do siebie np na telefon i dzialac offline

Technologie i jezyki oraz biblioteki:
Frontend -> React + Tailwind
Backend -> FastAPI (Python)
ML Model -> MobileNetV2 (TensorFlow + transfer learning)
Baza danych -> PostgreSQL (Supabase)
Hosting -> Railway / Render / Supabase / Vercel (frontend)
Offline -> PWA + TensorFlow Lite + SQLite



**Przechowywane dane o ziołach znajdują sie w _Herb-Regognition-Compressed.pdf_**
