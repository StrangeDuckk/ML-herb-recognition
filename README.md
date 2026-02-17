# 🌿 ML-herb-recognition 🌿
# 🌿 ML-rozpoznawanie-ziół 🌿
---
Projekt inżynierski łączący uczenie maszynowe, backend API oraz relacyjną bazę danych, umożliwiający rozpoznawanie roślin na podstawie cech morfologicznych oraz zdjęcia.

---
## Cel projektu
- Rozpoznawanie ziół na podstawie zdjęcia
- Wyszukiwanie roślin na podstawie filtrów morfologicznych
- Dodawanie nowych roślin do bazy (po zatwierdzeniu przez właściciela)
- Przeglądanie właściwości zdrowotnych i produktów powiązanych z rośliną
- Działanie online na stronie web
- Zwracanie top 5 wyników na podstawie danych wprowadzonych przez uzytkownika
---
## Design Bazy danych
Baza została maksymalnie znormalizowana aby umożliwic dokladne odwzorowanie cech morfologicznych roślin oraz umożliwić filtrowanie po cechach. 

Baza uzmozliwia tez pokazanie powiazan pomiedzy rozlinami a produktami
Większość niezbeznych danych zbiera sie w tabeli **PLANTS** co umozliwia optymalny sposob pisania zapytan, bez niepotrzebnego rozbudowania

---
## Po co?

Aplikacja dla osób które chcą znaleźć najszersze źródło wiedzy o ziołach razem z przepisami i ich właściwościami. 
Wszystko w jednym miejscu bez potrzeby przeszukiwania wielu aplikacji
Bez ograniczenia tylko do roślin leczniczych.

Aplikacja składajaca sie z bazy danych, połączeń API oraz strony Web do przeglądania danych o roślinach leczniczych.

Tryb rozpoznawania aplikacji:
Uzytkownik poprzez wypełnianie formularza wprowadza dane o napotkanej roślinie i następnie wysyła zapytanie do bazy. Baza przetwarza jego zapytanie i zwraca 5 najbardziej prawdopodobnych wyników.


**Sposób przechowywania danych o ziołach znajduje sie w _Herb-Regognition-Compressed.pdf_**
