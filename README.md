# IceClicker – Windows EXE (C#)

Detectează un obiect dintr-o captură de ecran folosind OpenCV template matching și apasă click stânga când potrivirea depășește pragul setat.

## Compilare EXE pe GitHub (fără Python)

1. Creează un repository nou pe GitHub.
2. Încarcă **toate fișierele și folderele** acestui proiect (inclusiv `.github/workflows/build.yml` și `assets/target.png`).
3. Deschide tabul **Actions** → **Build Windows EXE** → **Run workflow**.
4. După terminarea jobului, descarcă arhiva **IceClicker-Windows-x64** din secțiunea **Artifacts**.
5. Dezarhivează și rulează `IceClicker.exe`. Păstrează folderul `assets` lângă executabil.

## Comenzi

- Butonul din fereastră sau F8 (când fereastra este activă): pornire/oprire.
- F9 (când fereastra este activă): ieșire.
- Pragul (%) controlează cât de asemănătoare trebuie să fie imaginea.
- Pauza (ms) limitează frecvența clickurilor.

## Limitări

- Potrivirea este pe baza imaginii statice, la scări între 55% și 145%. Nu este un model AI și poate rata obiecte rotite, acoperite, cu iluminare diferită sau poate face clickuri greșite.
- În modul fullscreen exclusiv sau în jocuri cu protecție anti-cheat, capturarea ori clickul simulat pot să nu funcționeze. Nu include bypass de protecții.
- Cursorul este mutat la centrul imaginii detectate. Testează întâi într-un mediu sigur.
- Tastele F8/F9 funcționează numai când fereastra programului este activă.
- Nu este testat prin compilare în acest mediu; GitHub Actions va raporta orice eroare de build.
