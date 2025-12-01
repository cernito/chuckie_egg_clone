# Ukázky použití

K použití programu není potřeba žádných vstupních souborů od uživatele. Ovládání hry je popsáno v [uživatelské dokumentaci](docs/user.md) v sekci "Ovládání".

Úrovně hry jsou načítány automaticky ze souborů ve složce `Resources/Levels/` (např. `level1.txt`, `level2.txt` až `level8.txt`). Každý soubor obsahuje ASCII reprezentaci mapy dané úrovně.

Například `level1.txt`:
```
22
14
######################
#....................#
#......OHEf...Of....O#
#.....==H==.======.==#
#.......H.Of....f....#
#....OE.H.==...==....#
#...====H...O==.f.O..#
#..OHef.H..==...====.#
#..=H===H==..........#
#.O.H.feH...H.Of.H.O.#
#.======H===H====H==.#
#..f.O..H.P.Hef..H...#
#====================#
######################
```

Na prvním a druhém řádku je uvedena šířka a výška mapy.
Jednotlivé znaky znamenají:
- `=` - plošina
- `H` - žebřík
- `P` - hráč
- `O` - vajíčko
- `E` - nepřítel (malá kachna)
- `e` - další nepřítel (malá kachna) - přidává se v pozdější fázi hry
- `f` - jídlo (kukuřice) 
- `L` - pohyblivá plošina