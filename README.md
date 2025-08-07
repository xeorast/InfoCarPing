# Uwaga
Zgodnie z regulaminem strony info-car.pl obowiązującym od dnia 14.04.2023 r ([archiwum](https://web.archive.org/web/20230605075318/https://info-car.pl/new/assets/i18n/pl.json?v=1.2.8), sekcja TERMS_OF_SERVICE_PAGE):
> **§6. Usługa Sprawdź dostępność terminów egzaminu na prawo jazdy**
> 
> 2. Zabrania się wykorzystywania usługi w celu zautomatyzowanego pobierania danych o wolnych terminach egzaminów na prawo jazdy i prezentowania ich w usługach zewnętrznych niezależnych od Administratora.

W związku z tym kod zawarty w tym repozytorium służy ***wyłącznie do celów edukacyjnych***, a z racji użycia w obecnej wersji strony systemu Cloudflare Turnstile (odmiana CAPTCHA) kod po uruchomieniu wbrew regulaminowi ***nie będzie działał***.

Kod ***może służyć jako baza*** do programów operujących na stronach dopuszczających dostęp botów. Autor nie ponosi odpowiedzialności za ewentualne konsekwencje prób użycia kodu w obecnej formie.

# O aplikacji
Miejsca na egzamin na termin 2-3 dni do przodu zwalniają się często, nawet kilka dziennie, ale znikają w przeciągu 3-4 minut. 

Apka konsolowa co minutę wykonuje zapytanie do info-car.pl o dostępne terminy egzaminu praktycznego. W przypadku znalezienia wolnego terminu w oczekiwanym zakresie dat pokazuje go w konsoli i informuje brzęczykiem, umożliwiając szybką reakcję i zajęcie terminu. 

***Apka nie rezerwuje sama miejsc, a jedynie informuje o ich dostępności.*** 

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
