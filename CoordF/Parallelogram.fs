module Parallelogram

open Coord
open System.Windows
open System.Windows.Media
open System.IO

type plgm = vec2 * base2

module plgm =

    let b008784 = [| 1; 2; 5; 10; 13; 17; 25; 26; 29; 34; 37; 41; 50; 53; 58; 61; 65; 73; 74; 82; 85; 89; 97; 101; 106; 109; 113; 122; 125; 130; 137; 145; 146; 149; 157; 169; 170; 173; 178; 181; 185; 193; 194; 197; 202; 205; 218; 221; 226; 229; 233; 241; 250; 257; 265; 269; 274; 277; 281; 289; 290; 293; 298; 305; 313; 314; 317; 325; 337; 338; 346; 349; 353; 362; 365; 370; 373; 377; 386; 389; 394; 397; 401; 409; 410; 421; 425; 433; 442; 445; 449; 457; 458; 461; 466; 481; 482; 485; 493; 505; 509; 514; 521; 530; 533; 538; 541; 545; 554; 557; 562; 565; 569; 577; 578; 586; 593; 601; 610; 613; 617; 625; 626; 629; 634; 641; 650; 653; 661; 673; 674; 677; 685; 689; 697; 698; 701; 706; 709; 725; 730; 733; 745; 746; 754; 757; 761; 769; 773; 778; 785; 793; 794; 797; 802; 809; 818; 821; 829; 841; 842; 845; 850; 853; 857; 865; 866; 877; 881; 890; 898; 901; 905; 914; 922; 925; 929; 937; 941; 949; 953; 962; 965; 970; 977; 985; 986; 997; 1009; 1010; 1013; 1018; 1021; 1025; 1033; 1037; 1042; 1049; 1061; 1066; 1069; 1073; 1082; 1090; 1093; 1097; 1105; 1109; 1114; 1117; 1129; 1130; 1138; 1145; 1153; 1154; 1157; 1165; 1181; 1186; 1189; 1193; 1201; 1202; 1205; 1213; 1217; 1226; 1229; 1234; 1237; 1241; 1249; 1250; 1258; 1261; 1277; 1282; 1285; 1289; 1297; 1301; 1306; 1313; 1321; 1322; 1325; 1345; 1346; 1354; 1361; 1369; 1370; 1373; 1378; 1381; 1385; 1394; 1402; 1405; 1409; 1417; 1418; 1429; 1433; 1445; 1450; 1453; 1465; 1466; 1469; 1481; 1489; 1490; 1493; 1513; 1514; 1517; 1522; 1525; 1537; 1538; 1546; 1549; 1553; 1565; 1570; 1585; 1586; 1594; 1597; 1601; 1609; 1613; 1618; 1621; 1625; 1637; 1642; 1649; 1657; 1658; 1669; 1681; 1682; 1685; 1690; 1693; 1697; 1706; 1709; 1714; 1717; 1721; 1730; 1733; 1741; 1745; 1753; 1754; 1762; 1765; 1769; 1777; 1781; 1789; 1801; 1802; 1810; 1825; 1850; 1853; 1858; 1861; 1865; 1873; 1874; 1877; 1882; 1885; 1889; 1898; 1901; 1906; 1913; 1921; 1930; 1933; 1937; 1945; 1949; 1954; 1961; 1970; 1973; 1985; 1993; 1994; 1997; 2005; 2017; 2018; 2026; 2029; 2041; 2042; 2045; 2050; 2053; 2066; 2069; 2074; 2081; 2089; 2098; 2105; 2113; 2117; 2122; 2125; 2129; 2137; 2138; 2141; 2146; 2153; 2161; 2165; 2173; 2186; 2194; 2197; 2210; 2213; 2218; 2221; 2225; 2234; 2237; 2245; 2249; 2257; 2258; 2269; 2273; 2281; 2285; 2290; 2293; 2297; 2305; 2306; 2309; 2314; 2329; 2330; 2333; 2341; 2353; 2357; 2362; 2377; 2378; 2381; 2386; 2389; 2393; 2402; 2405; 2410; 2417; 2425; 2426; 2434; 2437; 2441; 2458; 2465; 2473; 2474; 2477; 2482; 2498; 2501; 2509; 2521; 2522; 2525; 2533; 2545; 2549; 2554; 2557; 2561; 2570; 2578; 2581; 2593; 2594; 2602; 2605; 2609; 2617; 2621; 2626; 2633; 2642; 2650; 2657; 2665; 2669; 2677; 2689; 2690; 2693; 2701; 2705; 2713; 2722; 2725; 2729; 2738; 2741; 2746; 2749; 2753; 2762; 2770; 2777; 2785; 2789; 2797; 2801; 2809; 2810; 2813; 2818; 2825; 2833; 2834; 2837; 2845; 2857; 2858; 2861; 2866; 2873; 2885; 2890; 2897; 2906; 2909; 2917; 2929; 2930; 2938; 2941; 2953; 2957; 2962; 2965; 2969; 2977; 2978; 2986; 2993; 3001; 3005; 3026; 3029; 3034; 3037; 3041; 3049; 3050; 3061; 3065; 3074; 3077; 3085; 3089; 3098; 3106; 3109; 3121; 3125; 3130; 3133; 3137; 3145; 3161; 3169; 3170; 3181; 3194; 3202; 3205; 3209; 3217; 3218; 3221; 3226; 3229; 3233; 3242; 3250; 3253; 3257; 3265; 3274; 3277; 3281; 3293; 3298; 3301; 3305; 3313; 3314; 3329; 3338; 3341; 3349; 3361; 3362; 3365; 3370; 3373; 3385; 3386; 3389; 3394; 3413; 3418; 3425; 3433; 3434; 3442; 3445; 3449; 3457; 3461; 3466; 3469; 3482; 3485; 3490; 3497; 3505; 3506; 3517; 3529; 3530; 3533; 3538; 3541; 3545; 3554; 3557; 3562; 3578; 3581; 3589; 3593; 3601; 3602; 3613; 3617; 3625; 3637; 3649; 3650; 3653; 3665; 3673; 3677; 3697; 3701; 3706; 3709; 3721; 3722; 3725; 3730; 3733; 3737; 3746; 3754; 3757; 3761; 3769; 3770; 3778; 3785; 3793; 3797; 3802; 3805; 3809; 3821; 3826; 3833; 3842; 3845; 3853; 3865; 3866; 3869; 3874; 3877; 3881; 3889; 3890; 3893; 3898; 3917; 3922; 3925; 3929; 3946; 3961; 3965; 3970; 3973; 3977; 3985; 3986; 3989; 3994; 4001; 4010; 4013; 4021; 4033; 4034; 4045; 4049; 4057; 4058; 4069; 4073; 4082; 4090; 4093; 4097; 4105; 4106; 4121; 4129; 4133; 4138; 4141; 4145; 4153; 4157; 4162; 4177; 4178; 4181; 4201; 4205; 4210; 4217; 4225; 4226; 4229; 4234; 4241; 4250; 4253; 4258; 4261; 4265; 4273; 4274; 4282; 4285; 4289; 4297; 4306; 4321; 4322; 4325; 4330; 4337; 4346; 4349; 4357; 4369; 4373; 4381; 4385; 4394; 4397; 4405; 4409; 4421; 4426; 4441; 4442; 4450; 4453; 4457; 4469; 4474; 4481; 4490; 4493; 4498; 4505; 4513; 4514; 4517; 4525; 4537; 4538; 4546; 4549; 4553; 4561; 4562; 4570; 4573; 4586; 4589; 4594; 4597; 4610; 4618; 4621; 4625; 4633; 4637; 4645; 4649; 4657; 4658; 4666; 4673; 4682; 4685; 4705; 4706; 4709; 4714; 4717; 4721; 4729; 4733; 4745; 4754; 4762; 4765; 4777; 4778; 4786; 4789; 4793; 4801; 4810; 4813; 4817; 4825; 4834; 4849; 4850; 4861; 4874; 4877; 4882; 4885; 4889; 4901; 4909; 4913; 4925; 4930; 4933; 4937; 4946; 4954; 4957; 4969; 4973; 4981; 4985; 4993; 5002; 5009; 5017; 5018; 5021; 5042; 5045; 5050; 5057; 5065; 5066; 5069; 5077; 5081; 5090; 5098; 5101; 5105; 5113; 5114; 5122; 5125; 5141; 5153; 5161; 5162; 5165; 5185; 5186; 5189; 5197; 5209; 5210; 5213; 5218; 5233; 5234; 5237; 5242; 5245; 5249; 5261; 5266; 5273; 5281; 5297; 5305; 5309; 5314; 5317; 5321; 5329; 5330; 5333; 5338; 5345; 5353; 5354; 5365; 5378; 5381; 5386; 5389; 5393; 5402; 5410; 5413; 5417; 5426; 5429; 5437; 5441; 5449; 5450; 5458; 5465; 5473; 5477; 5482; 5485; 5498; 5501; 5506; 5513; 5521; 5525; 5545; 5554; 5557; 5569; 5570; 5573; 5578; 5581; 5585; 5594; 5597; 5602; 5617; 5618; 5626; 5629; 5641; 5645; 5650; 5653; 5657; 5666; 5669; 5674; 5689; 5690; 5693; 5701; 5713; 5714; 5717; 5722; 5725; 5729; 5737; 5741; 5746; 5749; 5765; 5770; 5777; 5785; 5794; 5801; 5809; 5813; 5818; 5821; 5825; 5834; 5837; 5849; 5857; 5858; 5861; 5869; 5881; 5882; 5897; 5905; 5906; 5914; 5917; 5930; 5933; 5938; 5941; 5945 |]
    let b057756 = [| 0; 1; 2; 3; 5; 4; 7; 5; 12; 13; 6; 9; 7; 23; 17; 11; 8; 27; 31; 9; 13; 34; 22; 10; 23; 33; 15; 11; 57; 47; 37; 12; 27; 44; 28; 70; 13; 80; 55; 19; 43; 81; 75; 14; 91; 32; 33; 21; 15; 107; 89; 64; 57; 16; 23; 82; 37; 60; 53; 38; 17; 138; 105; 72; 25; 129; 114; 18; 148; 99; 93; 136; 42; 19; 27; 43; 104; 70; 81; 115; 183; 63; 20; 143; 73; 29; 132; 179; 21; 123; 67; 109; 107; 48; 89; 31; 177; 22; 157; 192; 208; 241; 235; 23; 73; 187; 52; 33; 217; 118; 53; 98; 86; 24; 251; 155; 77; 125; 133; 35; 194; 182; 25; 191; 203; 154; 57; 149; 106; 58; 189; 26; 37; 83; 132; 213; 135; 311; 96; 157; 27; 353; 193; 269; 99; 87; 39; 62; 317; 115; 28; 255; 63; 215; 381; 318; 143; 295; 246; 41; 29; 268; 157; 333; 207; 93; 179; 151; 387; 123; 67; 30; 162; 109; 413; 43; 324; 196; 97; 265; 442; 31; 112; 313; 252; 183; 157; 161; 469; 293; 45; 301; 374; 32; 355; 72; 235; 426; 103; 73; 249; 191; 489; 33; 530; 341; 47; 354; 439; 214; 168; 437; 483; 107; 140; 553; 34; 322; 243; 77; 278; 186; 49; 125; 177; 495; 78; 35; 597; 423; 546; 319; 585; 443; 191; 216; 113; 487; 273; 479; 36; 51; 149; 515; 257; 555; 182; 82; 615; 651; 614; 117; 37; 668; 83; 366; 217; 319; 135; 53; 452; 294; 613; 620; 542; 38; 157; 497; 138; 353; 437; 465; 225; 193; 432; 55; 87; 401; 39; 682; 394; 707; 317; 88; 339; 288; 443; 203; 255; 215; 610; 40; 523; 127; 491; 166; 57; 316; 295; 463; 783; 583; 220; 378; 41; 148; 437; 92; 414; 333; 390; 207; 293; 473; 93; 410; 59; 213; 713; 151; 387; 42; 133; 775; 174; 724; 824; 659; 343; 557; 43; 251; 605; 61; 477; 737; 741; 137; 97; 278; 331; 265; 218; 511; 712; 98; 467; 598; 44; 663; 589; 725; 401; 183; 259; 63; 834; 161; 412; 782; 229; 469; 45; 992; 499; 647; 143; 893; 244; 355; 164; 599; 102; 789; 623; 392; 65; 46; 103; 557; 372; 296; 249; 419; 191; 232; 147; 612; 401; 563; 341; 239; 47; 1083; 755; 790; 568; 903; 1021; 67; 772; 438; 961; 982; 290; 710; 348; 107; 600; 365; 48; 1013; 688; 411; 174; 377; 108; 153; 200; 633; 243; 1134; 829; 69; 1007; 285; 971; 49; 512; 177; 592; 507; 495; 1139; 398; 672; 597; 157; 567; 691; 915; 319; 585; 50; 112; 71; 463; 293; 701; 208; 357; 113; 611; 408; 273; 479; 568; 918; 1261; 51; 807; 389; 667; 472; 515; 1224; 257; 507; 163; 73; 914; 550; 1142; 187; 859; 265; 52; 887; 747; 1057; 1102; 117; 656; 705; 640; 794; 1015; 217; 190; 118; 167; 603; 1258; 500; 53; 75; 957; 693; 1357; 905; 416; 483; 896; 809; 1202; 891; 268; 553; 327; 1120; 497; 878; 54; 394; 1017; 437; 599; 1226; 1222; 465; 77; 964; 122; 225; 1061; 173; 1353; 1077; 55; 788; 401; 281; 774; 475; 743; 501; 578; 447; 200; 423; 393; 1461; 339; 727; 79; 1068; 1227; 177; 56; 302; 360; 1325; 203; 282; 987; 1561; 487; 484; 1436; 523; 234; 127; 839; 560; 1455; 57; 1598; 291; 802; 1321; 128; 81; 746; 463; 1212; 767; 407; 783; 1600; 1449; 1269; 183; 900; 1303; 58; 863; 1105; 703; 1601; 1344; 1283; 1471; 1319; 1407; 1651; 293; 473; 83; 1122; 708; 1453; 1323; 1003; 59; 132; 213; 187; 1267; 713; 596; 808; 1017; 548; 133; 852; 613; 775; 943; 785; 1065; 364; 216; 1153; 60; 977; 85; 1234; 307; 1027; 1034; 557; 1071; 353; 994; 1309; 1131; 1279; 251; 1609; 682; 61; 193; 477; 851; 697; 737; 137; 616; 604; 1445; 307; 331; 87; 803; 742; 1683; 722; 138; 376; 1201; 361; 693; 62; 1305; 317; 1335; 1560; 1087; 502; 197; 454; 663; 336; 589; 835; 401; 443; 226; 259; 89; 538; 63; 1607; 1239; 582; 1159; 481; 1585; 899; 1183; 1230; 723; 142; 229; 318; 884; 1857; 1037; 1227; 549; 499; 143; 1059; 64; 1347; 1809; 203; 895; 733; 1905; 91; 583; 1643; 1761; 1979; 457; 789; 919; 1154; 882; 813; 1911; 268; 65; 2082; 1433; 1044; 557; 561; 1757; 721; 333; 1200; 1841; 419; 207; 528; 1972; 1921; 1148; 147; 93; 687; 886; 401; 608; 66; 1526; 1904; 148; 1028; 239; 505; 387; 332; 952; 1083; 2146; 1431; 657; 538; 1880; 360; 1021; 276; 67; 2213; 785; 242; 95; 1597; 1474; 343; 213; 1287; 1983; 1260; 1699; 2205; 1571; 1023; 1696; 1693; 395; 365; 2129; 413; 1621; 152; 68; 1567; 2044; 1253; 1846; 1912; 1407; 2225; 1993; 153; 1133; 97; 343; 217; 633; 500; 1697; 1365; 897; 538; 1243; 69; 442; 1177; 285; 971; 1481; 1480; 1403; 697; 1868; 1291; 1432; 1825; 642; 507; 493; 2039; 719; 1769; 252; 730; 70; 1613; 1985; 1168; 157; 1194; 849; 567; 915; 359; 1076; 223; 2189; 1158; 158; 255; 539; 945; 853; 1363; 71; 1478; 293; 1282; 968; 701; 1881; 858; 2412; 717; 357; 101; 647; 2025; 611; 577; 1057; 507; 227; 460; 945; 678; 72; 1675; 2446; 1969; 2098; 807; 421; 389; 2253; 667; 369; 2149; 623; 162; 827; 1409; 944; 1673; 2313; 103; 1804; 163; 1084; 914; 776; 73; 2630; 1441; 1318; 394; 2127; 302; 1547; 1739; 859; 837; 665; 265; 593; 429; 368; 887; 233; 630; 2452; 635; 1057; 1627; 563; 450; 74; 2085; 1438; 2109; 1115; 1959; 105; 765; 268; 1463; 2587; 2478; 973; 1553; 2017; 167; 1437; 903; 603; 853; 1543; 237; 2309; 75; 1045; 1429; 168; 693; 310; 1670; 1357; 1046; 2421; 2124; 483; 1193; 385; 1984; 1961; 2416; 1659; 107; 863; 1126; 2378; 1789; 806; 1013; 553; 76; 1123; 1777; 1145; 1856; 796; 2031; 1242; 843; 2863; 382; 2839; 1310; 1525; 754; 1042; 1098; 599; 543; 243; 1727; 1735; 172; 77; 2656; 2005; 109; 278 |]

    let mthop h =
        b008784
        |> Array.tryFindIndex (fun a -> sqrt <| float a > h)
        |> Option.map (fun n ->
            let w, pl =
                if n = 0 then 1.0, true else
                let p = 1.0 / (sqrt <| float b008784.[n-1])
                let q = (h + (float b057756.[n]) * (sqrt <| float (float b008784.[n] - h * h))) / (float b008784.[n])
                if q < p then q, false else p, true
            n, w, pl)

    let mth = mthop >> Option.map (fun (n, w, pl) -> w) >> Option.defaultValue nan

    let powoflog slow dp = 
        let q, _ = (dp - 1) /% 3
        if slow then 10 * q else 10 * (q + 1)
    let powofstep mstep slow = mstep |> log10 |> int |> (~-) |> powoflog slow

    let rots = dict [
                        for dp in 0 .. 12 do
                            for pw in 0 .. powoflog false dp do
                                let s = (pown 2.0 pw) * (pown 10.0 -dp) * tau / 360.0
                                let sin, cos = sin s, cos s
                                yield s, (fun (v : vec2) -> let x, y = v.x, v.y in { x = cos * x - sin * y; y = sin * x + cos * y })
                                yield -s, (fun (v : vec2) -> let x, y = v.x, v.y in { x = cos * x + sin * y; y = - sin * x + cos * y })
                    ]

    let init w h = ({ x = (1.0 - w) / 2.0 ; y = -h / 2.0 }, (vec2.x w, vec2.y h))

    let fromvertices o a b = (o, (a - o, b - o))

    let normbase ((_, bas) : plgm) = base2.norm bas
    
    let contains p ((o, bas) : plgm) =
        let x, y = base2.coordinates bas (p - o)
        x > 0.0 && x < 1.0 && y > 0.0 && y < 1.0
    
    let sectorcontains p ((o, (u, v)) : plgm) =
        let op = p - o
        if op .* op < max (u .* u) (v .* v) then
            let x, y = base2.coordinates (u, v) op
            x > 0.0 && x < 1.0 && y > 0.0 && y < 1.0
        else false
    
    let translate t ((o, (u, v)) : plgm) = o + t, (u, v)
    let scale c su sv ((o, (u, v)) : plgm) =
        let ocx, ocy = c - o |> base2.decompose (u, v)
        o + (1.0 - su) * ocx + (1.0 - sv) * ocy, (su * u, sv * v)
    let rotate c a ((o, (u, v)) : plgm) =
        let r = let sc, rt = rots.TryGetValue a in if sc then rt else vec2.rotate a
        let o2 = r (o - c)
        let u2, v2 = r u, r v
        (o2 + c, (u2, v2))
    
    let sortinbase bas ((o, (u, v)) : plgm) =
        let mmx ux vx = match sign ux, sign vx with
                        | -1, -1 -> o + u + v, o
                        | 1, 1 -> o, o + u + v
                        | su, sv -> if su < sv then o + u, o + v else o + v, o + u
        let ux, uy = base2.coordinates bas u
        let vx, vy = base2.coordinates bas v
        let l, r = mmx ux vx
        let b, t = mmx uy vy
        l, b, r, t
        
    let sort = sortinbase (base2.ij)

    let miniboxofpoints (points : vec2 list) =
        let xs = points |> List.minBy (fun p -> p.x)
        let ys = points |> List.minBy (fun p -> p.y)
        let xe = points |> List.maxBy (fun p -> p.x)
        let ye = points |> List.maxBy (fun p -> p.y)
        xs, ys, xe, ye
    
    let minibox ((o, (u, v)) : plgm) =
        let l, b, r, t = sort (o, (u, v))
        l.x, b.y, r.x, t.y

    let treesinminibox (xs, ys, xe, ye) =
        seq { for x = int (floor xs) + 1 to int (ceil xe) - 1 do
                for y = int (floor ys) + 1 to int (ceil ye) - 1 do
                    { x = float x; y = float y} }
    
    let treesarroundplgm data = data |> minibox |> treesinminibox

    let treesinplgm data = data |> treesarroundplgm |> Seq.choose (fun tree -> if contains tree data then Some tree else None)
    let treesinsector data = data |> treesarroundplgm |> Seq.choose (fun tree -> if sectorcontains tree data then Some tree else None)
    
    let containstrees data = data |> treesinplgm |> Seq.isEmpty |> not
    
    let bycsm (csm : ReadOnlyCoordinatesSystemManager) ((o, (u, v)) : plgm) =
        let o2 = Point(o.x, o.y) |*> csm
        let u2 = csm.ComputeOutCoordinates(Vector(u.x, u.y))
        let v2 = csm.ComputeOutCoordinates(Vector(v.x, v.y))
        { x = o2.X ; y = o2.Y }, ({ x = u2.X ; y = u2.Y }, { x = v2.X ; y = v2.Y })
    
    let geometry ((o, (u, v)) : plgm) =
        let a, b, c = o + u, o + v, o + u + v
        let sr = StreamGeometry()
        use ctxt = sr.Open()
        ctxt.BeginFigure(Point(o.x, o.y), true, true)
        ctxt.LineTo(Point(a.x, a.y), true, true)
        ctxt.LineTo(Point(c.x, c.y), true, true)
        ctxt.LineTo(Point(b.x, b.y), true, true)
        sr

    let trajitr tr ((o, (u, v)) : plgm) =
        let _, b, _, t = sortinbase (base2.orthdir tr) (o, (u, v))
        (b, (tr, t - b))

    let trajirotim cr ang ((o, (u, v)) : plgm) =
        let a, b, c = o + u, o + v, o + u + v
        let (op, (up, vp)) = rotate cr ang (o, (u, v))
        let ap, bp, cp = op + up, op + vp, op + up + vp
        (op, (up, vp)), fromvertices cr a ap, fromvertices cr b bp, fromvertices cr c cp, fromvertices cr o op
    
    let translateOrNot tr data =
        if trajitr tr data |> treesinplgm |> Seq.isEmpty then
            let newdata = translate tr data
            if treesinplgm newdata |> Seq.isEmpty then Some newdata
            else None
        else None
    
    let rotateOrNot cr ang data =
        let newdata, s1, s2, s3, s4 = trajirotim cr ang data
        if treesinplgm newdata |> Seq.isEmpty 
           && treesinsector s1 |> Seq.isEmpty 
           && treesinsector s2 |> Seq.isEmpty 
           && treesinsector s3 |> Seq.isEmpty 
           && treesinsector s4 |> Seq.isEmpty then Some newdata
        else None

    let translatemax tr ((o, (u, v)) : plgm) =
        let (op, _) = translate tr (o, (u, v))
        let box = miniboxofpoints [ o; o + u; o + v; o + u + v; op; op + u; op + v; op + u + v ]
        ()
    
    let translateOrNotIL step data = translateOrNot (vec2.x (-step)) data
    let translateOrNotIR step data = translateOrNot (vec2.x (step)) data
    let translateOrNotJD step data = translateOrNot (vec2.y (-step)) data
    let translateOrNotJU step data = translateOrNot (vec2.y (step)) data
    
    let translateOrNotUL (step : float) ((o, (u, v)) : plgm) =
        translateOrNot (-step * (vec2.norm u)) (o, (u, v))
    let translateOrNotUR (step : float)((o, (u, v)) : plgm) =
           translateOrNot (step * (vec2.norm u)) (o, (u, v))
    let translateOrNotVD (step : float)((o, (u, v)) : plgm) =
           translateOrNot (-step * (vec2.norm v)) (o, (u, v))
    let translateOrNotVU (step : float) ((o, (u, v)) : plgm) =
           translateOrNot (step * (vec2.norm v)) (o, (u, v))
    
    let rotateOrNotD c step data = rotateOrNot c (step * tau / 360.0) data
    let rotateOrNotH c step data = rotateOrNot c (-step * tau / 360.0) data
    
    let rotateOrNotAtCenterD step ((o, (u, v)) : plgm) = rotateOrNotD (o + u / 2.0 + v / 2.0) step (o, (u, v))
    let rotateOrNotAtCenterH step ((o, (u, v)) : plgm) = rotateOrNotH (o + u / 2.0 + v / 2.0) step (o, (u, v))
    
    let translateIL step data = translate (vec2.x (-step)) data
    let translateIR step data = translate (vec2.x (step)) data
    let translateJD step data = translate (vec2.y (-step)) data
    let translateJU step data = translate (vec2.y (step)) data
    
    let translateUL (step : float) ((o, (u, v)) : plgm) =
        translate (-step * (vec2.norm u)) (o, (u, v))
    let translateUR (step : float)((o, (u, v)) : plgm) =
           translate (step * (vec2.norm u)) (o, (u, v))
    let translateVD (step : float)((o, (u, v)) : plgm) =
           translate (-step * (vec2.norm v)) (o, (u, v))
    let translateVU (step : float) ((o, (u, v)) : plgm) =
           translate (step * (vec2.norm v)) (o, (u, v))
    
    let rotateD c step data = rotate c (step * tau / 360.0) data
    let rotateH c step data = rotate c (-step * tau / 360.0) data
    
    let rotateAtCenterD step ((o, (u, v)) : plgm) = rotateD (o + u / 2.0 + v / 2.0) step (o, (u, v))
    let rotateAtCenterH step ((o, (u, v)) : plgm) = rotateH (o + u / 2.0 + v / 2.0) step (o, (u, v))

    let opstk mlog onnext hz slow =
        let rec opn n op data =
            if n = 0 then data
            else match op data with
                 | Some newdata -> opn (n - 1) op newdata
                 | None -> data

        let rec opsn n op data =
            async {
                let newdata = opn n op data
                if newdata = data then return data
                else
                    do! onnext newdata
                    return! opsn n op newdata
            }
        
        let rec ops op step data =
            async {
                match op step data with
                | Some ((o, (u, v)) : plgm) ->
                    do! onnext (o, (u, v))
                    if hz && v.y <= 0.0 then return (o, (u, v))
                    else return! ops op step (o, (u, v))
                | None -> return data
            }

        let mstep = pown 10.0 mlog
        let pw = powofstep mstep slow
        let st = pown 2.0 pw

        let opsfast op data =
            let rec opsf step op data =
                async {
                    if step <= mstep then return data
                    else
                        let! aops = ops op step data
                        return! opsf (step / 2.0) op aops
                }
            opsf (st * mstep) op data
            
        let trtomid optr ((o, bas) : plgm) =
            async {
                let! no, _ = opsfast optr (o, bas)
                let res = ((o + no) / 2.0, bas)
                do! onnext res
                return res
            }

        ops, opn, opsn, opsfast, trtomid
    
    let horiz1 (_, _, _, opsfast, _) data =

            let rsf c = opsfast (rotateOrNotH c)
            let isf = opsfast translateOrNotUR
            let usf = opsfast translateOrNotVU
    
            let rec rbs data = 
                let o, (u, v) = data
                async {
                    let! ars1 = rsf (o + u + v) data
                    if ars1 = data then return! rsf (o + v) data
                    else return ars1
                }

            let rec ris data =
                async {
                    let! ars = rbs data
                    let! ais = isf ars
                    if ais = ars then return ais
                    else return! ris ais
                }
    
            let rec hz ((o, (u, v)) : plgm) =
                async {
                    if v.y <= 0.0 then
                        return (o, (u, v)), true
                    else
                        let! aris = ris (o, (u, v))
                        let! aus = usf aris
                        if aus = aris then 
                            return aus, false
                        else return! hz aus
                }
            hz data
    
    let horiz2 (_, _, _, opsfast, trtomid) data =

            let rf = opsfast rotateOrNotAtCenterH
            let uf = opsfast translateOrNotUR
            let um = trtomid translateOrNotUL
            let vf = opsfast translateOrNotVU
            let vm = trtomid translateOrNotVD
    
            let rec hz ((o, (u, v)) : plgm) =
                async {
                    if v.y <= 0.0 then
                        return (o, (u, v)), true
                    else
                        let! arf = rf (o, (u, v))
                        let! auf = uf arf
                        let! avf = vf auf
                        let! aum = um avf
                        let! (no, (nu, nv)) = vm aum
                        if u = nu then 
                            return (no, (nu, nv)), false
                        else return! hz (no, (nu, nv))
                }
            hz data

    let ncs w h astep astart aend csm =
        let b = vec2.x w, vec2.y h
        let ox, oy = -w / 2.0, -h / 2.0
        let l = w * w + h * h
        let ls, le = int <| ceil -l, int <| floor l
        let trees = [ for i in  ls .. le do for j in  ls .. le do yield double i, double j ]
        let plgms =
            trees
            |> List.filter (fun (x, y) -> x * x + y * y <= l)
            |> List.map (fun (x, y) -> let p = new Point (x, y) |*> csm in (p.X, p.Y), ({ x = x + ox ; y = y + oy }, b) |> bycsm csm |> geometry)
        let g = (vec2.zero, (vec2.i, vec2.j)) |> bycsm csm |> geometry
        let rec nc a acc =
            if a > aend then true, acc
            else
                let u = plgms |> List.fold (fun s ((cx, cy), p) -> p.Transform <- new RotateTransform (a, cx, cy); Geometry.Combine (s, p, GeometryCombineMode.Union, null) :> Geometry) Geometry.Empty
                let z = Geometry.Combine (g, u, GeometryCombineMode.Exclude, null)
                if z.Bounds = Rect.Empty then false, acc
                else nc (a + astep) (z :: acc)
        nc astart []

    let ncss w h astep astart aend =
        let b = vec2.x w, vec2.y h
        let ox, oy = -w / 2.0, -h / 2.0
        let l = w * w + h * h
        let ls, le = int <| ceil -l, int <| floor l
        let trees = [ for i in  ls .. le do for j in  ls .. le do yield double i, double j ]
        let plgms =
            trees
            |> List.filter (fun (x, y) -> x * x + y * y <= l)
            |> List.map (fun (x, y) -> (x, y), ({ x = x + ox ; y = y + oy }, b) |> geometry)
        let g = (vec2.zero, (vec2.i, vec2.j)) |> geometry
        let rec nc a =
            if a > aend then true, nan
            else
                let u = plgms |> List.fold (fun s ((cx, cy), p) -> p.Transform <- new RotateTransform (a, cx, cy); Geometry.Combine (s, p, GeometryCombineMode.Union, null) :> Geometry) Geometry.Empty
                let z = Geometry.Combine (g, u, GeometryCombineMode.Exclude, null)
                if z.Bounds = Rect.Empty then false, a
                else nc (a + astep)
        nc astart

    let horizmaxwidth1 height optk (wstep : decimal) (wstart : decimal) =
            let rec hmw width =
                async {
                    let! _, success = horiz2 optk (init (double width) height)
                    if success then return width
                    else let w = width - wstep in return! hmw w
                }
            hmw wstart

    let horizmaxwidth2 height optk (wstep : decimal) (wstart : decimal) =
            let rec hmw width data =
                async {
                    let! (o, (u, v)), success = horiz2 optk data
                    if success then return width
                    else let w = width - wstep in return! hmw w (o, (vec2.relength (double w) u, v))
                }
            hmw wstart (init (double wstart) height)

    let horizmaxwidths (hstart : decimal) (hend : decimal) (hstep : decimal) optk (wstep : decimal) (wstart : decimal) =
        let total = int ((hend - hstart) / hstep) + 1
        let rec hmw h ws hst n acc =
            async {
                let nh = h + hst
                if nh > hend then 
                    if hst = hstep then return acc
                    else return! hmw h ws (hst / 2M) (n - (int (hst / hstep) / 2)) acc
                else
                    if hst = hstep then
                        let! w = horizmaxwidth1 (double nh) optk wstep ws
                        printfn "%.9f : %.9f ---- %d / %d" nh w n total
                        if w = ws then return! hmw nh w (1048576M * hstep) (n + 1048576) ((nh, w) :: acc)
                        else return! hmw nh w hstep (n + 1) ((nh, w) :: acc)
                    else
                        let! _, s = horiz2 optk (init (double ws) (double nh))
                        if s then
                            printfn "%.9f : %.9f ---- %d / %d" nh ws n total
                            if hst = 1048576M * hstep then return! hmw nh ws hst (n + 1048576) ((nh, ws) :: acc)
                            else return! hmw nh ws (hst / 2M) (n + (int (hst / hstep) / 2)) ((nh, ws) :: acc)
                        else return! hmw h ws (hst / 2M) (n - (int (hst / hstep) / 2)) acc
            }
        async {
            printfn "Hauteurs dans [ %.9f , %.9f ] avec un pas de %.9f" hstart hend hstep
            let! res = hmw hstart wstart hstep 1 []
            use fstr = File.Open ("res.txt", FileMode.Create)
            fstr.Flush ()
            use sr = new StreamWriter (fstr :> Stream)
            res
            |> List.map (fun (h, w) -> sprintf "%.9f : %.9f" h w)
            |> List.iter (fun s -> sr.WriteLine s)
            return res
        }
        

