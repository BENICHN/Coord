module Parallelogram

open Coord
open System.Windows
open System.Windows.Media
open System.IO

type plgm = vec2 * base2

module plgm =

    let b008784 = [| 1.0; 2.0; 5.0; 10.0; 13.0; 17.0; 25.0; 26.0; 29.0; 34.0; 37.0; 41.0; 50.0; 53.0; 58.0; 61.0; 65.0; 73.0; 74.0; 82.0; 85.0; 89.0; 97.0; 101.0; 106.0; 109.0; 113.0; 122.0; 125.0; 130.0; 137.0; 145.0; 146.0; 149.0; 157.0; 169.0; 170.0; 173.0; 178.0; 181.0; 185.0; 193.0; 194.0; 197.0; 202.0; 205.0; 218.0; 221.0; 226.0; 229.0; 233.0; 241.0; 250.0; 257.0; 265.0; 269.0; 274.0; 277.0; 281.0; 289.0; 290.0; 293.0; 298.0; 305.0; 313.0; 314.0; 317.0; 325.0; 337.0; 338.0; 346.0; 349.0; 353.0; 362.0; 365.0; 370.0; 373.0; 377.0; 386.0; 389.0; 394.0; 397.0; 401.0; 409.0; 410.0; 421.0; 425.0; 433.0; 442.0; 445.0; 449.0; 457.0; 458.0; 461.0; 466.0; 481.0; 482.0; 485.0; 493.0; 505.0; 509.0; 514.0; 521.0; 530.0; 533.0; 538.0; 541.0; 545.0; 554.0; 557.0; 562.0; 565.0; 569.0; 577.0; 578.0; 586.0; 593.0; 601.0; 610.0; 613.0; 617.0; 625.0; 626.0; 629.0; 634.0; 641.0; 650.0; 653.0; 661.0; 673.0; 674.0; 677.0; 685.0; 689.0; 697.0; 698.0; 701.0; 706.0; 709.0; 725.0; 730.0; 733.0; 745.0; 746.0; 754.0; 757.0; 761.0; 769.0; 773.0; 778.0; 785.0; 793.0; 794.0; 797.0; 802.0; 809.0; 818.0; 821.0; 829.0; 841.0; 842.0; 845.0; 850.0; 853.0; 857.0; 865.0; 866.0; 877.0; 881.0; 890.0; 898.0; 901.0; 905.0; 914.0; 922.0; 925.0; 929.0; 937.0; 941.0; 949.0; 953.0; 962.0; 965.0; 970.0; 977.0; 985.0; 986.0; 997.0; 1009.0; 1010.0; 1013.0; 1018.0; 1021.0; 1025.0; 1033.0; 1037.0; 1042.0; 1049.0; 1061.0; 1066.0; 1069.0; 1073.0; 1082.0; 1090.0; 1093.0; 1097.0; 1105.0; 1109.0; 1114.0; 1117.0; 1129.0; 1130.0; 1138.0; 1145.0; 1153.0; 1154.0; 1157.0; 1165.0; 1181.0; 1186.0; 1189.0; 1193.0; 1201.0; 1202.0; 1205.0; 1213.0; 1217.0; 1226.0; 1229.0; 1234.0; 1237.0; 1241.0; 1249.0; 1250.0; 1258.0; 1261.0; 1277.0; 1282.0; 1285.0; 1289.0; 1297.0; 1301.0; 1306.0; 1313.0; 1321.0; 1322.0; 1325.0; 1345.0; 1346.0; 1354.0; 1361.0; 1369.0; 1370.0; 1373.0; 1378.0; 1381.0; 1385.0; 1394.0; 1402.0; 1405.0; 1409.0; 1417.0; 1418.0; 1429.0; 1433.0; 1445.0; 1450.0; 1453.0; 1465.0; 1466.0; 1469.0; 1481.0; 1489.0; 1490.0; 1493.0; 1513.0; 1514.0; 1517.0; 1522.0; 1525.0; 1537.0; 1538.0; 1546.0; 1549.0; 1553.0; 1565.0; 1570.0; 1585.0; 1586.0; 1594.0; 1597.0; 1601.0; 1609.0; 1613.0; 1618.0; 1621.0; 1625.0; 1637.0; 1642.0; 1649.0; 1657.0; 1658.0; 1669.0; 1681.0; 1682.0; 1685.0; 1690.0; 1693.0; 1697.0; 1706.0; 1709.0; 1714.0; 1717.0; 1721.0; 1730.0; 1733.0; 1741.0; 1745.0; 1753.0; 1754.0; 1762.0; 1765.0; 1769.0; 1777.0; 1781.0; 1789.0; 1801.0; 1802.0; 1810.0; 1825.0; 1850.0; 1853.0; 1858.0; 1861.0; 1865.0; 1873.0; 1874.0; 1877.0; 1882.0; 1885.0; 1889.0; 1898.0; 1901.0; 1906.0; 1913.0; 1921.0; 1930.0; 1933.0; 1937.0; 1945.0; 1949.0; 1954.0; 1961.0; 1970.0; 1973.0; 1985.0; 1993.0; 1994.0; 1997.0; 2005.0; 2017.0; 2018.0; 2026.0; 2029.0; 2041.0; 2042.0; 2045.0; 2050.0; 2053.0; 2066.0; 2069.0; 2074.0; 2081.0; 2089.0; 2098.0; 2105.0; 2113.0; 2117.0; 2122.0; 2125.0; 2129.0; 2137.0; 2138.0; 2141.0; 2146.0; 2153.0; 2161.0; 2165.0; 2173.0; 2186.0; 2194.0; 2197.0; 2210.0; 2213.0; 2218.0; 2221.0; 2225.0; 2234.0; 2237.0; 2245.0; 2249.0; 2257.0; 2258.0; 2269.0; 2273.0; 2281.0; 2285.0; 2290.0; 2293.0; 2297.0; 2305.0; 2306.0; 2309.0; 2314.0; 2329.0; 2330.0; 2333.0; 2341.0; 2353.0; 2357.0; 2362.0; 2377.0; 2378.0; 2381.0; 2386.0; 2389.0; 2393.0; 2402.0; 2405.0; 2410.0; 2417.0; 2425.0; 2426.0; 2434.0; 2437.0; 2441.0; 2458.0; 2465.0; 2473.0; 2474.0; 2477.0; 2482.0; 2498.0; 2501.0; 2509.0; 2521.0; 2522.0; 2525.0; 2533.0; 2545.0; 2549.0; 2554.0; 2557.0; 2561.0; 2570.0; 2578.0; 2581.0; 2593.0; 2594.0; 2602.0; 2605.0; 2609.0; 2617.0; 2621.0; 2626.0; 2633.0; 2642.0; 2650.0; 2657.0; 2665.0; 2669.0; 2677.0; 2689.0; 2690.0; 2693.0; 2701.0; 2705.0; 2713.0; 2722.0; 2725.0; 2729.0; 2738.0; 2741.0; 2746.0; 2749.0; 2753.0; 2762.0; 2770.0; 2777.0; 2785.0; 2789.0; 2797.0; 2801.0; 2809.0; 2810.0; 2813.0; 2818.0; 2825.0; 2833.0; 2834.0; 2837.0; 2845.0; 2857.0; 2858.0; 2861.0; 2866.0; 2873.0; 2885.0; 2890.0; 2897.0; 2906.0; 2909.0; 2917.0; 2929.0; 2930.0; 2938.0; 2941.0; 2953.0; 2957.0; 2962.0; 2965.0; 2969.0; 2977.0; 2978.0; 2986.0; 2993.0; 3001.0; 3005.0; 3026.0; 3029.0; 3034.0; 3037.0; 3041.0; 3049.0; 3050.0; 3061.0; 3065.0; 3074.0; 3077.0; 3085.0; 3089.0; 3098.0; 3106.0; 3109.0; 3121.0; 3125.0; 3130.0; 3133.0; 3137.0; 3145.0; 3161.0; 3169.0; 3170.0; 3181.0; 3194.0; 3202.0; 3205.0; 3209.0; 3217.0; 3218.0; 3221.0; 3226.0; 3229.0; 3233.0; 3242.0; 3250.0; 3253.0; 3257.0; 3265.0; 3274.0; 3277.0; 3281.0; 3293.0; 3298.0; 3301.0; 3305.0; 3313.0; 3314.0; 3329.0; 3338.0; 3341.0; 3349.0; 3361.0; 3362.0; 3365.0; 3370.0; 3373.0; 3385.0; 3386.0; 3389.0; 3394.0; 3413.0; 3418.0; 3425.0; 3433.0; 3434.0; 3442.0; 3445.0; 3449.0; 3457.0; 3461.0; 3466.0; 3469.0; 3482.0; 3485.0; 3490.0; 3497.0; 3505.0; 3506.0; 3517.0; 3529.0; 3530.0; 3533.0; 3538.0; 3541.0; 3545.0; 3554.0; 3557.0; 3562.0; 3578.0; 3581.0; 3589.0; 3593.0; 3601.0; 3602.0; 3613.0; 3617.0; 3625.0; 3637.0; 3649.0; 3650.0; 3653.0; 3665.0; 3673.0; 3677.0; 3697.0; 3701.0; 3706.0; 3709.0; 3721.0; 3722.0; 3725.0; 3730.0; 3733.0; 3737.0; 3746.0; 3754.0; 3757.0; 3761.0; 3769.0; 3770.0; 3778.0; 3785.0; 3793.0; 3797.0; 3802.0; 3805.0; 3809.0; 3821.0; 3826.0; 3833.0; 3842.0; 3845.0; 3853.0; 3865.0; 3866.0; 3869.0; 3874.0; 3877.0; 3881.0; 3889.0; 3890.0; 3893.0; 3898.0; 3917.0; 3922.0; 3925.0; 3929.0; 3946.0; 3961.0; 3965.0; 3970.0; 3973.0; 3977.0; 3985.0; 3986.0; 3989.0; 3994.0; 4001.0; 4010.0; 4013.0; 4021.0; 4033.0; 4034.0; 4045.0; 4049.0; 4057.0; 4058.0; 4069.0; 4073.0; 4082.0; 4090.0; 4093.0; 4097.0; 4105.0; 4106.0; 4121.0; 4129.0; 4133.0; 4138.0; 4141.0; 4145.0; 4153.0; 4157.0; 4162.0; 4177.0; 4178.0; 4181.0; 4201.0; 4205.0; 4210.0; 4217.0; 4225.0; 4226.0; 4229.0; 4234.0; 4241.0; 4250.0; 4253.0; 4258.0; 4261.0; 4265.0; 4273.0; 4274.0; 4282.0; 4285.0; 4289.0; 4297.0; 4306.0; 4321.0; 4322.0; 4325.0; 4330.0; 4337.0; 4346.0; 4349.0; 4357.0; 4369.0; 4373.0; 4381.0; 4385.0; 4394.0; 4397.0; 4405.0; 4409.0; 4421.0; 4426.0; 4441.0; 4442.0; 4450.0; 4453.0; 4457.0; 4469.0; 4474.0; 4481.0; 4490.0; 4493.0; 4498.0; 4505.0; 4513.0; 4514.0; 4517.0; 4525.0; 4537.0; 4538.0; 4546.0; 4549.0; 4553.0; 4561.0; 4562.0; 4570.0; 4573.0; 4586.0; 4589.0; 4594.0; 4597.0; 4610.0; 4618.0; 4621.0; 4625.0; 4633.0; 4637.0; 4645.0; 4649.0; 4657.0; 4658.0; 4666.0; 4673.0; 4682.0; 4685.0; 4705.0; 4706.0; 4709.0; 4714.0; 4717.0; 4721.0; 4729.0; 4733.0; 4745.0; 4754.0; 4762.0; 4765.0; 4777.0; 4778.0; 4786.0; 4789.0; 4793.0; 4801.0; 4810.0; 4813.0; 4817.0; 4825.0; 4834.0; 4849.0; 4850.0; 4861.0; 4874.0; 4877.0; 4882.0; 4885.0; 4889.0; 4901.0; 4909.0; 4913.0; 4925.0; 4930.0; 4933.0; 4937.0; 4946.0; 4954.0; 4957.0; 4969.0; 4973.0; 4981.0; 4985.0; 4993.0; 5002.0; 5009.0; 5017.0; 5018.0; 5021.0; 5042.0; 5045.0; 5050.0; 5057.0; 5065.0; 5066.0; 5069.0; 5077.0; 5081.0; 5090.0; 5098.0; 5101.0; 5105.0; 5113.0; 5114.0; 5122.0; 5125.0; 5141.0; 5153.0; 5161.0; 5162.0; 5165.0; 5185.0; 5186.0; 5189.0; 5197.0; 5209.0; 5210.0; 5213.0; 5218.0; 5233.0; 5234.0; 5237.0; 5242.0; 5245.0; 5249.0; 5261.0; 5266.0; 5273.0; 5281.0; 5297.0; 5305.0; 5309.0; 5314.0; 5317.0; 5321.0; 5329.0; 5330.0; 5333.0; 5338.0; 5345.0; 5353.0; 5354.0; 5365.0; 5378.0; 5381.0; 5386.0; 5389.0; 5393.0; 5402.0; 5410.0; 5413.0; 5417.0; 5426.0; 5429.0; 5437.0; 5441.0; 5449.0; 5450.0; 5458.0; 5465.0; 5473.0; 5477.0; 5482.0; 5485.0; 5498.0; 5501.0; 5506.0; 5513.0; 5521.0; 5525.0; 5545.0; 5554.0; 5557.0; 5569.0; 5570.0; 5573.0; 5578.0; 5581.0; 5585.0; 5594.0; 5597.0; 5602.0; 5617.0; 5618.0; 5626.0; 5629.0; 5641.0; 5645.0; 5650.0; 5653.0; 5657.0; 5666.0; 5669.0; 5674.0; 5689.0; 5690.0; 5693.0; 5701.0; 5713.0; 5714.0; 5717.0; 5722.0; 5725.0; 5729.0; 5737.0; 5741.0; 5746.0; 5749.0; 5765.0; 5770.0; 5777.0; 5785.0; 5794.0; 5801.0; 5809.0; 5813.0; 5818.0; 5821.0; 5825.0; 5834.0; 5837.0; 5849.0; 5857.0; 5858.0; 5861.0; 5869.0; 5881.0; 5882.0; 5897.0; 5905.0; 5906.0; 5914.0; 5917.0; 5930.0; 5933.0; 5938.0; 5941.0; 5945.0 |]
    let b057756 = [| 0.0; 1.0; 2.0; 3.0; 5.0; 4.0; 7.0; 5.0; 12.0; 13.0; 6.0; 9.0; 7.0; 23.0; 17.0; 11.0; 8.0; 27.0; 31.0; 9.0; 13.0; 34.0; 22.0; 10.0; 23.0; 33.0; 15.0; 11.0; 57.0; 47.0; 37.0; 12.0; 27.0; 44.0; 28.0; 70.0; 13.0; 80.0; 55.0; 19.0; 43.0; 81.0; 75.0; 14.0; 91.0; 32.0; 33.0; 21.0; 15.0; 107.0; 89.0; 64.0; 57.0; 16.0; 23.0; 82.0; 37.0; 60.0; 53.0; 38.0; 17.0; 138.0; 105.0; 72.0; 25.0; 129.0; 114.0; 18.0; 148.0; 99.0; 93.0; 136.0; 42.0; 19.0; 27.0; 43.0; 104.0; 70.0; 81.0; 115.0; 183.0; 63.0; 20.0; 143.0; 73.0; 29.0; 132.0; 179.0; 21.0; 123.0; 67.0; 109.0; 107.0; 48.0; 89.0; 31.0; 177.0; 22.0; 157.0; 192.0; 208.0; 241.0; 235.0; 23.0; 73.0; 187.0; 52.0; 33.0; 217.0; 118.0; 53.0; 98.0; 86.0; 24.0; 251.0; 155.0; 77.0; 125.0; 133.0; 35.0; 194.0; 182.0; 25.0; 191.0; 203.0; 154.0; 57.0; 149.0; 106.0; 58.0; 189.0; 26.0; 37.0; 83.0; 132.0; 213.0; 135.0; 311.0; 96.0; 157.0; 27.0; 353.0; 193.0; 269.0; 99.0; 87.0; 39.0; 62.0; 317.0; 115.0; 28.0; 255.0; 63.0; 215.0; 381.0; 318.0; 143.0; 295.0; 246.0; 41.0; 29.0; 268.0; 157.0; 333.0; 207.0; 93.0; 179.0; 151.0; 387.0; 123.0; 67.0; 30.0; 162.0; 109.0; 413.0; 43.0; 324.0; 196.0; 97.0; 265.0; 442.0; 31.0; 112.0; 313.0; 252.0; 183.0; 157.0; 161.0; 469.0; 293.0; 45.0; 301.0; 374.0; 32.0; 355.0; 72.0; 235.0; 426.0; 103.0; 73.0; 249.0; 191.0; 489.0; 33.0; 530.0; 341.0; 47.0; 354.0; 439.0; 214.0; 168.0; 437.0; 483.0; 107.0; 140.0; 553.0; 34.0; 322.0; 243.0; 77.0; 278.0; 186.0; 49.0; 125.0; 177.0; 495.0; 78.0; 35.0; 597.0; 423.0; 546.0; 319.0; 585.0; 443.0; 191.0; 216.0; 113.0; 487.0; 273.0; 479.0; 36.0; 51.0; 149.0; 515.0; 257.0; 555.0; 182.0; 82.0; 615.0; 651.0; 614.0; 117.0; 37.0; 668.0; 83.0; 366.0; 217.0; 319.0; 135.0; 53.0; 452.0; 294.0; 613.0; 620.0; 542.0; 38.0; 157.0; 497.0; 138.0; 353.0; 437.0; 465.0; 225.0; 193.0; 432.0; 55.0; 87.0; 401.0; 39.0; 682.0; 394.0; 707.0; 317.0; 88.0; 339.0; 288.0; 443.0; 203.0; 255.0; 215.0; 610.0; 40.0; 523.0; 127.0; 491.0; 166.0; 57.0; 316.0; 295.0; 463.0; 783.0; 583.0; 220.0; 378.0; 41.0; 148.0; 437.0; 92.0; 414.0; 333.0; 390.0; 207.0; 293.0; 473.0; 93.0; 410.0; 59.0; 213.0; 713.0; 151.0; 387.0; 42.0; 133.0; 775.0; 174.0; 724.0; 824.0; 659.0; 343.0; 557.0; 43.0; 251.0; 605.0; 61.0; 477.0; 737.0; 741.0; 137.0; 97.0; 278.0; 331.0; 265.0; 218.0; 511.0; 712.0; 98.0; 467.0; 598.0; 44.0; 663.0; 589.0; 725.0; 401.0; 183.0; 259.0; 63.0; 834.0; 161.0; 412.0; 782.0; 229.0; 469.0; 45.0; 992.0; 499.0; 647.0; 143.0; 893.0; 244.0; 355.0; 164.0; 599.0; 102.0; 789.0; 623.0; 392.0; 65.0; 46.0; 103.0; 557.0; 372.0; 296.0; 249.0; 419.0; 191.0; 232.0; 147.0; 612.0; 401.0; 563.0; 341.0; 239.0; 47.0; 1083.0; 755.0; 790.0; 568.0; 903.0; 1021.0; 67.0; 772.0; 438.0; 961.0; 982.0; 290.0; 710.0; 348.0; 107.0; 600.0; 365.0; 48.0; 1013.0; 688.0; 411.0; 174.0; 377.0; 108.0; 153.0; 200.0; 633.0; 243.0; 1134.0; 829.0; 69.0; 1007.0; 285.0; 971.0; 49.0; 512.0; 177.0; 592.0; 507.0; 495.0; 1139.0; 398.0; 672.0; 597.0; 157.0; 567.0; 691.0; 915.0; 319.0; 585.0; 50.0; 112.0; 71.0; 463.0; 293.0; 701.0; 208.0; 357.0; 113.0; 611.0; 408.0; 273.0; 479.0; 568.0; 918.0; 1261.0; 51.0; 807.0; 389.0; 667.0; 472.0; 515.0; 1224.0; 257.0; 507.0; 163.0; 73.0; 914.0; 550.0; 1142.0; 187.0; 859.0; 265.0; 52.0; 887.0; 747.0; 1057.0; 1102.0; 117.0; 656.0; 705.0; 640.0; 794.0; 1015.0; 217.0; 190.0; 118.0; 167.0; 603.0; 1258.0; 500.0; 53.0; 75.0; 957.0; 693.0; 1357.0; 905.0; 416.0; 483.0; 896.0; 809.0; 1202.0; 891.0; 268.0; 553.0; 327.0; 1120.0; 497.0; 878.0; 54.0; 394.0; 1017.0; 437.0; 599.0; 1226.0; 1222.0; 465.0; 77.0; 964.0; 122.0; 225.0; 1061.0; 173.0; 1353.0; 1077.0; 55.0; 788.0; 401.0; 281.0; 774.0; 475.0; 743.0; 501.0; 578.0; 447.0; 200.0; 423.0; 393.0; 1461.0; 339.0; 727.0; 79.0; 1068.0; 1227.0; 177.0; 56.0; 302.0; 360.0; 1325.0; 203.0; 282.0; 987.0; 1561.0; 487.0; 484.0; 1436.0; 523.0; 234.0; 127.0; 839.0; 560.0; 1455.0; 57.0; 1598.0; 291.0; 802.0; 1321.0; 128.0; 81.0; 746.0; 463.0; 1212.0; 767.0; 407.0; 783.0; 1600.0; 1449.0; 1269.0; 183.0; 900.0; 1303.0; 58.0; 863.0; 1105.0; 703.0; 1601.0; 1344.0; 1283.0; 1471.0; 1319.0; 1407.0; 1651.0; 293.0; 473.0; 83.0; 1122.0; 708.0; 1453.0; 1323.0; 1003.0; 59.0; 132.0; 213.0; 187.0; 1267.0; 713.0; 596.0; 808.0; 1017.0; 548.0; 133.0; 852.0; 613.0; 775.0; 943.0; 785.0; 1065.0; 364.0; 216.0; 1153.0; 60.0; 977.0; 85.0; 1234.0; 307.0; 1027.0; 1034.0; 557.0; 1071.0; 353.0; 994.0; 1309.0; 1131.0; 1279.0; 251.0; 1609.0; 682.0; 61.0; 193.0; 477.0; 851.0; 697.0; 737.0; 137.0; 616.0; 604.0; 1445.0; 307.0; 331.0; 87.0; 803.0; 742.0; 1683.0; 722.0; 138.0; 376.0; 1201.0; 361.0; 693.0; 62.0; 1305.0; 317.0; 1335.0; 1560.0; 1087.0; 502.0; 197.0; 454.0; 663.0; 336.0; 589.0; 835.0; 401.0; 443.0; 226.0; 259.0; 89.0; 538.0; 63.0; 1607.0; 1239.0; 582.0; 1159.0; 481.0; 1585.0; 899.0; 1183.0; 1230.0; 723.0; 142.0; 229.0; 318.0; 884.0; 1857.0; 1037.0; 1227.0; 549.0; 499.0; 143.0; 1059.0; 64.0; 1347.0; 1809.0; 203.0; 895.0; 733.0; 1905.0; 91.0; 583.0; 1643.0; 1761.0; 1979.0; 457.0; 789.0; 919.0; 1154.0; 882.0; 813.0; 1911.0; 268.0; 65.0; 2082.0; 1433.0; 1044.0; 557.0; 561.0; 1757.0; 721.0; 333.0; 1200.0; 1841.0; 419.0; 207.0; 528.0; 1972.0; 1921.0; 1148.0; 147.0; 93.0; 687.0; 886.0; 401.0; 608.0; 66.0; 1526.0; 1904.0; 148.0; 1028.0; 239.0; 505.0; 387.0; 332.0; 952.0; 1083.0; 2146.0; 1431.0; 657.0; 538.0; 1880.0; 360.0; 1021.0; 276.0; 67.0; 2213.0; 785.0; 242.0; 95.0; 1597.0; 1474.0; 343.0; 213.0; 1287.0; 1983.0; 1260.0; 1699.0; 2205.0; 1571.0; 1023.0; 1696.0; 1693.0; 395.0; 365.0; 2129.0; 413.0; 1621.0; 152.0; 68.0; 1567.0; 2044.0; 1253.0; 1846.0; 1912.0; 1407.0; 2225.0; 1993.0; 153.0; 1133.0; 97.0; 343.0; 217.0; 633.0; 500.0; 1697.0; 1365.0; 897.0; 538.0; 1243.0; 69.0; 442.0; 1177.0; 285.0; 971.0; 1481.0; 1480.0; 1403.0; 697.0; 1868.0; 1291.0; 1432.0; 1825.0; 642.0; 507.0; 493.0; 2039.0; 719.0; 1769.0; 252.0; 730.0; 70.0; 1613.0; 1985.0; 1168.0; 157.0; 1194.0; 849.0; 567.0; 915.0; 359.0; 1076.0; 223.0; 2189.0; 1158.0; 158.0; 255.0; 539.0; 945.0; 853.0; 1363.0; 71.0; 1478.0; 293.0; 1282.0; 968.0; 701.0; 1881.0; 858.0; 2412.0; 717.0; 357.0; 101.0; 647.0; 2025.0; 611.0; 577.0; 1057.0; 507.0; 227.0; 460.0; 945.0; 678.0; 72.0; 1675.0; 2446.0; 1969.0; 2098.0; 807.0; 421.0; 389.0; 2253.0; 667.0; 369.0; 2149.0; 623.0; 162.0; 827.0; 1409.0; 944.0; 1673.0; 2313.0; 103.0; 1804.0; 163.0; 1084.0; 914.0; 776.0; 73.0; 2630.0; 1441.0; 1318.0; 394.0; 2127.0; 302.0; 1547.0; 1739.0; 859.0; 837.0; 665.0; 265.0; 593.0; 429.0; 368.0; 887.0; 233.0; 630.0; 2452.0; 635.0; 1057.0; 1627.0; 563.0; 450.0; 74.0; 2085.0; 1438.0; 2109.0; 1115.0; 1959.0; 105.0; 765.0; 268.0; 1463.0; 2587.0; 2478.0; 973.0; 1553.0; 2017.0; 167.0; 1437.0; 903.0; 603.0; 853.0; 1543.0; 237.0; 2309.0; 75.0; 1045.0; 1429.0; 168.0; 693.0; 310.0; 1670.0; 1357.0; 1046.0; 2421.0; 2124.0; 483.0; 1193.0; 385.0; 1984.0; 1961.0; 2416.0; 1659.0; 107.0; 863.0; 1126.0; 2378.0; 1789.0; 806.0; 1013.0; 553.0; 76.0; 1123.0; 1777.0; 1145.0; 1856.0; 796.0; 2031.0; 1242.0; 843.0; 2863.0; 382.0; 2839.0; 1310.0; 1525.0; 754.0; 1042.0; 1098.0; 599.0; 543.0; 243.0; 1727.0; 1735.0; 172.0; 77.0; 2656.0; 2005.0; 109.0; 278.0 |]

    let mthop h = b008784 |> Array.tryFindIndex (fun a -> sqrt a > h) |> Option.map (fun n -> if n = 0 then 1.0 else min (1.0 / (sqrt b008784.[n-1])) ((h + b057756.[n] * sqrt (b008784.[n] - h * h)) / b008784.[n]))
    let mth = mthop >> Option.defaultValue nan

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
        

