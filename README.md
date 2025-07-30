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



# przechowywane dane o ziolach
- nazwy
    - polska
    - lacinska
    - angielska
    - przydomki
- zdjecie
- gatunek
    - drzewo / kwiat / krzew / grzyb
- wyglad    
    - jesli wybrany w garunku: kwiat:
        - korzen
            -gruby/twardy/wiotki/kruchy
        - lodyga
            -brak
            -posiada
                - ksztalt
                    - okragla / prostokatna / szesciokatna / nieregularna
                - gladka 
                    - tak / nie
                - kolce
                    - brak
                    - wystepuja
                        - miekkie / twarde / z wloskami / czepne
                - twardosc
                    -twarda / lamliwa / gietka / sztywna / miekka
                - kolor
                    - zielony / czarny / zdrewnialy / srebrny / czerwony / inny (wpisanie)
                - sok
                    - brak
                    - posiada
                        - lepkosc
                            - lepki jak guma / lepki ja maka z woda / nie lepki
                        - zapach
                            - brak / trup / slodki / kwiatowy / inny (wpisanie)
                        - kolor
                            - bialy / zolty / inny (wpisanie)
                        - brudzenie
                            - nie brudzi / brudzi
                - dlugosc
                    - krotka (do 10 cm) / srednia (do 50 cm) / pien drzewa (od 51 cm +)
        - liscie
            - brak
            - posiada
                - ksztalt
                    - okragle / podlozne / drobne / poszarpane / nieregularny 
                - wielksoc
                    - drobne (do 1 cm dlugosci pojedynczego liscia) / srednie (do 5 cm) / dlugie (ponad 5 cm dlugosci)
                - wyglad
                    - miesiste / lamliwe / grube / kolczaste / sprezyste / drobne
                - kolor
                    - zielone / srebrny / czerwona obwodka / zielona obwodka / inne
                - sok
                    -brak
                    -posiada
                        - kolor
                            - bialy / zolty / przezroczysty / inny (wpisz)
                        - brudzenie
                            - nie brudzi / brudzi
        - kwiat
            - brak
            - posiada
                - wielkosc
                    - drobne (do 1 cm srednicy) / srednie (do 10 cm srednicy) / duze (do 20 cm srednicy) / ogromne (powyzej 20 cm srednicy)
                - kolor
                    - bialy / zolty / czerwony / rozowy / niebieski (lub fioletowy) / inny (wpisz)
                - zapach
                    - brak
                    - posiada
                        - mocny / delikatny / gryzacy / kojacy / inny (wpisz)
                - zawartosc
                    - brak
                    - pusty (powietrze) / nasiona
        - nasiona
            - brak
            - wystepuja
                - ksztalt
                    - okragle / podlozne / patyczkowate
                - wielkosc
                    - drobne (jak makowe) / srednie (jak nasiona dmuchawca) / dlugie (jak kozibrodu (duzy dmuchawiec))
     - jesli wybrany w gatunku: grzyb:
        - trzon
            - kolor
                - bialy / szary / brazowy / inny (wpisz)
            - wystepowanie pierscienia
                - brak
                - wystepuje
                    - staly / ruchomy
            - gladkosc
                - gladki / wzorzysty
            - grubosc 
                - wezszy niz palec / szerszy niz palec
            - dlugosc 
                - do 5 cm / wyzszy niz 5 cm
        - kapelusz
            - spod kapelusza
                - blaszki / gabka
            - gladkosc
                - gladki / chropowaty / z plamkami / sliski i lepki
            - kolor
                - brazowy / czerwony / bialy / szary / zolty (pomaranczowy) / inny (wpisz) 
                - kropki
                    - brak
                    - posiada
                        - czerwone / biale / brazowe 
    - informacje dodatkowe
- wystepowanie
    - podmokle
    - woda
    - suche / pola
    - skraj lasu / miedze
    - zacienione / las
- wlasciwosci (selcka tylko do informacji zwrotnej)
    - zdrowotny
        - wzmacniajacy / przeciwwirusowy / przeciwpasorzytniczy / przeciwgoraczkowy / przeciwbolowy / odkarzajacy / przeciwbakteryjny / uspokajajacy / ... (mozliwe wiele na raz)
        - w jakiej dawce trujacy
    - trujacy
        - atakuje zoladek / atakuje nerki / atakuje uklad nerwowy / atakuje uklad krwionosny / atakuje pluca (zaciska oskrzela) / niewydolnosc wielonarzadowa / nasenne
        - stopien trucia (smiertelny lub nie)
        - dawka smiertelna / trujaca jesli wiadoma
    - nie stwierdzono
- wyroby (sekcja tylko do informacji zwrotnej)
    - masci
        - przepis ogolny + dzialanie + jak stosowac
    - nalewki 
        - przepis ogolny + dzialanie + jak stosowac
    - susz
        - przepis ogolny + dzialanie + jak stosowac
    - wyciag
        - przepis ogolny + dzialanie + jak stosowac