# O aplikacji
Miejsca na egzamin na termin 2-3 dni do przodu zwalniają się często, nawet kilka dziennie, ale znikają w przeciągu 3-4 minut. 

Apka konsolowa co minutę wykonuje zapytanie do info-car.pl o dostępne terminy egzaminu praktycznego. W przypadku znalezienia wolnego terminu w oczekiwanym zakresie dat pokazuje go w konsoli i informuje brzęczykiem, umożliwiając szybką reakcję i zajęcie terminu. 

***Apka nie rezerwuje sama miejsc, a jedynie informuje o ich dostępności.*** 
Dla najlepszego efektu zalecam przygotowanie wcześniej w przeglądarce wypełnionego formularza, by musieć tylko kliknąć datę i przesłać formularz.

# Aby uruchomić:

Zmień nazwę pliku `example-config.json` na `config.json`, upewnij się, że kopiuje się on do folderu wyjściowego. Plik ma następujący format:
```json
{
	"Category": "B",
	"Username": "login@gmail.com",
	"Password": "p@ssw0rd",
	"WordId": 42,
	"MaxWaitingTimeInDays": 10,
	"MinExamTime": "6:00",
	"MaxExamTime": "21:00"
}
```

Wpisz w nim swój login i hasło do info-car.pl (nie musi być to to samo konto na którym będziesz rezerwować egzamin).

W polu `WordId` podaj id interesującego cię ośrodka WORD. Listę ośrodków razem z id można znaleźć [tutaj](https://info-car.pl/api/word/word-centers) w sekcji `words` (link działa bez logowania). Wyszukaj miejscowość swojego WORDu i odczytaj odpowiadające mu `id`.

W polu `MaxWaitingTimeInDays` wpisz jaki jest maksymalny czas czekania na egzamin o którym chcesz być powiadomiony. Przez `MinExamTime` i `MinExamTime` możesz ustawić zakres godzinowy w którym interesuje cię egzamin - te poza zakresem będą ignorowane. W polu `Category` możesz wybrać inną kategorię prawa jazdy.
