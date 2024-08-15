?BANK 08


-----------------------------

stairway_move_up:
  BEQ stairway_return
  DEC 
  STA $0026, Y
  DEC $26
  BEQ stairway_return
  RTL

stairway_move_down:
  BEQ stairway_return
  INC 
  STA $0026, Y
  DEC $26
  BEQ stairway_return
  RTL

stairway_return:
  JMP $C575
   
----------------------------

;Skip not equipped message
0384BF:
  RTS

03A126 #$0100  ;Plane crash delay

04B8E3 #30  ;Cave delay 1
04B8F6 #20  ;Cave delay 2
04B90C #80  ;Cave delay 3
04B919 #50  ;Cave delay 4

04B9AF `[DLG:3,6][SIZ:D,3]Suddenly Erik rushed in[N][LU1:F0]a desperate look[N]on his face.[CLD]`


;Skip flag checks for bread drop
04D234:
  BRA $0A

04D24A #$0100  ;Delay for bread drop
04D26E #$0100  ;Nightfall delay in cell
04D2D4 #$0080  ;Hamlet delay in cell


048AE9 `[DLG:3,6][SIZ:D,3][TPL:0]My name is Will.[FIN]A year has passed since[N]I [LU2:C1]to the Tower of[N]Babel [LU1:F0]my father.[FIN]My [LU2:67]and his party[N]met [LU1:F0]disaster.[FIN]Somehow, I [LU1:B3]it[N][LU1:73]to [LU1:47]Cape...[FIN]I [LU1:D3][LU1:78]believe[N]my [LU2:67]is gone.[N][LU1:1F][LU1:BB]believe it...[FIN][LU1:61]I grow up, I'll[N]be an [LU1:8A]and[N]see the world.[FIN]Somewhere, I [LU1:EF]meet[N]my father...[END]`

04D141 `[DEF][SFX:0][DLY:9][LU1:67][LU1:8F]a large,[N]yummy roast leg of yak![FIN][DEF][TPL:1][LU1:25][N]Everything's ready![N][LU1:2B]go [LU1:74]the[N][LU2:A2][LU1:90]us![PAL:0][END]`

04D732 `[DLG:3,11][SIZ:D,3]A familiar voice[N]speaks [LU1:8E]the flute.[FIN][TPL:E][TPL:4][LU2:E][N][LU1:3A][FIN][LU2:E][N]This is [LU1:FE]father.[END]`
04D78A `[TPL:E][TPL:0]Will: [N]Father...? [FIN][TPL:4][LU2:E]You were[N]a cute child, but now[N]you've grown up.[FIN]Isn't [LU1:17]Lola's[N]pie delicious?[FIN][TPL:0]Will: [N]Uh, sure, Dad![N][LU1:65]are you?![FIN][TPL:4][LU2:E][N]I [LU1:78][LU2:AD]you now...[FIN][TPL:F][TPL:4]I [LU1:98][LU1:C8]to ask[N]of [LU2:C7]Listen...[N][PAL:0] [LU1:6A]if [LU1:9F][LU1:FE]wish![N] No! You deserted me!`
04D87D `[CLD][TPL:F][TPL:4][CLR]Flute:[N]I [LU1:F1]you to[N][LU1:D1]me....[FIN]I, [LU2:B7]was once[N]held in [LU1:D6]cell.[N]Look at the[N][LU2:82]wall.[END]`


;04DB28:         ;Free up cell door flag (02)
;  COP [E0]

;04DB5D:	        ;Free up moss flag (03)
;  COP [E0]

;04DBEC:			;Free up chain flag (04)
;  COP [E0]
  
04F38D `[TPL:A][SFX:0][DLY:9][LU1:67]found[N]Incan [LU1:48]A.[PAU:B0][END]`
04FAF9 `[DLG:3,6][SIZ:D,3][SFX:0][DLY:9][LU1:67]got[N]Incan [LU1:48]B![PAU:B0][END]`

0592A6 #40  ;Ship tremor delay 1
0592CF #40  ;Ship tremor delay 2


0592FF `[TPL:E][TPL:4]Lance: [N][LU1:62][LU1:9D]to Seth? [N][LU1:4B]happened! [FIN][TPL:3]Erik: [N]Eeeeeeeh!!! [N][LU1:1D]Seth!!... [FIN]A huge, enormous, giant [N][LU2:62]ran [LU1:A1]the ship![N]Sob... [FIN]Seth fell in the [N]water! Sob... [FIN]He was swallowed!  [N]Gulp...Sob... [FIN][TPL:4]Lance: [N][LU1:62]was that?[PAL:0][END]`


0597C6 #$00B0  ;Raft day 1 delay
059AE5 #$9B33  ;Raft skip health check
059B73 #20     ;Raft night delay
059C02 #$00B0  ;Raft shark delay


059F5A `[TPL:A][TPL:0]Will: Time passed [N]slowly, [LU1:F0][LU1:B9][N]to break the monotony. [FIN][LU1:26][LU1:A3]stared at [N]the [LU2:62]all day. [N]Will couldn't stand it. [FIN]He walked [LU1:6D]on [N]the raft and talked to [N][LU1:26][LU1:B6]times. [FIN]A minute [LU2:A0][LU1:A5][N]forever. But he [LU1:7A][N][LU1:9B]the march of time.[PAL:0][END]`
05A232 `[TPL:A][TPL:1][LU1:25][N]................ [FIN][TPL:0]Will: [LU1:26][LU1:83][N]say [LU1:6C]all day.[FIN]A typical princess...[N][LU2:33][LU1:CE]a bother...[PAL:0][END]`
05A332 `[TPL:A][TPL:1][LU1:25][N]............... [FIN]Will... [N][LU2:35]I talked to you [N][LU1:D7]way yesterday.... [FIN][LU1:1F]try to eat the fish.[N]I [LU1:78]do anything[N]if I starve.[FIN][LU1:38]in peace [LU1:DF]can[N]you refuse food you[N][LU1:82]like...[FIN][TPL:0]Will: [N][LU1:2B]catch a fish. [N]A [LU1:94]one.[PAL:0][END]`
05A405 `[TPL:A][TPL:0]Will: [N]Happily [LU1:26]ate [LU1:D0][N]fish. [FIN]Will [LU1:8F][LU1:D7]he was [N]starting to develop [N]feelings for Kara...[PAL:0][END]`
05A5AB `[TPL:A][TPL:0]Will: I [LU2:71]for  [N][LU1:8B]safety, and [N]for my father...[PAL:0][END]`
05A9F9 `[TPL:A][TPL:0]Will: Suddenly Will[N]fell over,[N]unconscious... [FIN][TPL:1][LU1:25][N]Will! Will!! [N][LU1:64]wrong!! [FIN]Wake up!! Don't[N][LU1:AC]me [LU2:73]alone![PAL:0][END]`

05C85E `[TPL:B][TPL:4]Lance: [N][LU1:62]is [LU1:D6]place? [FIN][TPL:1][LU1:25][LU1:49]I [LU1:91][N]a [LU1:A7]homesick... [FIN][TPL:3]Erik: [N]I [LU1:91][LU1:A5]I'm [LU1:73]in [N]the womb.... [FIN][TPL:2][LU1:2A]Everything[N]that's [LU1:9D]and the[N][LU1:C1][LU1:20]met are[N]pouring [LU1:A1]my head...[FIN][TPL:4]Lance: I was [LU1:C5]in [N]the [LU1:E2]of [LU1:47]Cape. [FIN][LU1:61]my [LU2:67][N][LU1:83][LU1:79][LU1:73][N][LU1:8E]an expedition... [FIN]The [LU2:88]important [LU1:E3][N]in my [LU1:B0]was gone. I [N][LU1:83][LU1:A4][LU1:F7]to do. [FIN][TPL:1][LU1:25]I couldn't [N]stand my [LU2:67]using [N][LU2:A2]to invade [N][LU1:BD]countries. [FIN][LU1:1D]awful [LU1:F6]someone[N]loses [LU1:E1]life.[FIN][LU1:62]had [LU2:AF]years[N]to put together was[N][LU2:59]in one moment.[FIN][TPL:3]Erik: I [LU1:F9]if Seth [N]is all right...? [FIN][TPL:2][LU1:2A][LU1:3D][LU1:AF]on[N]because [LU2:AB]forget[N][LU1:6B]unpleasant things.[PAL:0][END]`

05E6C0 `[DEF][TPL:0]There's a tile buried[N]in the sand...[FIN][LU1:61]Will's Flute [N]touched it, [LU1:D9][N]was a rumbling sound![PAL:0][END]`
05E753 `[DEF][TPL:3]Erik: [N][LU2:15][LU1:4B][LU1:9C][N]is [LU1:81]down!![PAL:0][END]`
05E774 `[TPL:E][TPL:1][LU1:25][N]Will! Will! [N]Wi-i-i-i-i-i-l-l-l-l! [PAL:0][CLD]`

05EB4F `[DEF][TPL:6][LU1:36]Of course! [N]Cygnus has nine stars,[N]and [LU1:D9]are nine[N]stones... [FIN]`

05F160 `[DEF][TPL:2]Look! Look [LU1:F4]the[N]rocks are on the ground![FIN][LU1:5D]positioned like[N]the stars in the[N][LU1:7E]of Cygnus![PAL:0][END]`


0683C4 #00  ;Parachute delay


06B13C `[TPL:A][TPL:3]Erik: [N][LU1:30][LU1:9F]Riverson...? [FIN][TPL:1][LU1:25][N]Oh, no! [LU2:41][N]got to run!! [FIN][TPL:2]Lilly:[N]Run?[N]Run where?![PAL:0][END]`
06B250 `[TPL:A][TPL:5]This is Seth... [FIN][TPL:4]Lance: [N]Seth?!! [FIN][TPL:2]Lilly:[N]Shh. Quiet![N][LU2:23]continues. [FIN][TPL:5]I was swallowed by[N]Riverson...[FIN][LU1:61]I [LU1:80]to, the form[N]of my [LU1:72]had changed[N]to Riverson's.[FIN]This Riverson is a[N]creature who lives in[N]the ocean.[FIN]I [LU1:82]know[N]if [LU1:9F][LU1:9A]or not.[FIN]He [LU1:D4][LU1:D7]evolution[N]is [LU1:77]affected[N]by the [LU1:A8]of a comet.[FIN]I [LU1:FC]to continue[N]the journey [LU1:F0]you,[N]but not in [LU1:D6]body.[FIN]You [LU1:B2]figure [N]out [LU1:D6]riddle of the [N][LU1:7B]and the ruins...[PAL:0][END]`


;Watermia intro sequence
078009:
  COP [CC] ( #8D )
  COP [E0]
  
07944D `[DEF][SFX:10][TPL:4]Opponent:[N]One glass left...[WAI][CLD][DEF][TPL:6]Spectator:[N]That's enough...[N]This young man won...[FIN]Spectator:[N]Right![N][LU1:42]now![FIN][TPL:4]Opponent: No...[N]I'm the champion. I [N][LU1:EF]not be disgraced. [FIN][SFX:0][TPL:6]He picks up the glass.[END]`
079520 `[DEF][SFX:10][TPL:6]Spectator: Stop![N][LU1:67]already lost![N]Stop it!![FIN][SFX:0]Ignoring the spectator,[N]he downs the drink[N]in a shot.[PAL:0][END]`

  
079FB8 #80  ;Lily pad delay (bottom)
079FC7 #42 #04  ;Lily pad movement (frames, speed)
079FCE #42 #04  ;Lily pad movement (frames, speed)
079FE8 #60  ;Lily pad delay (top)
079FF7 #42 #03  ;Lily pad movement (frames, speed)
079FFE #42 #03  ;Lily pad movement (frames, speed)

07B7AE #$00C0  ;Great Wall cutscene delay

07B8AD `[TPL:A][TPL:4]Lance: [N]Oh. That [LU2:9D]. . . [FIN][TPL:0]Will: If you follow the [N][LU2:9D]chips, the trail [N]leads here.[FIN][LU1:1F][LU1:97][LU1:E6]back[N]to [LU2:C7][WAI][CLD]`
07B91A `[TPL:A][TPL:4][SFX:0]Lance whispers... [FIN]Lance: [N]Will...will you [LU1:E7][N][LU2:57]of [LU1:2C]for me? [END]`
07B960 `[TPL:A][TPL:4]Lance:[N]I was saved thanks[N]to [LU1:E4]stones...[FIN]This was the necklace [N]I [LU1:B3]for [LU2:C7][END]`
07B9B1 `[TPL:A][TPL:4]Lance: There aren't [LU1:B6][N]necklace stones left. [N]Will you [LU1:E7]them? [FIN][SFX:0][TPL:6]Lance, fixing the [N]necklace, puts it [N][LU1:6D]her neck. [END]`
07BA2E `[TPL:A][TPL:4]Lance: Wow!! [LU1:20][LU1:BB][N]felt [LU1:D6]way before![FIN][LU1:1D][LU1:A5]a million [N]summer days! [FIN][TPL:2]Lilly:[N](Sob).[N]I [LU1:91]the [LU1:CD]way.[FIN][LU1:2B]go [LU1:73]to the[N][LU2:BD]I'm sure[N][LU1:8B]worried.[END]`
07BBB0 `[TPL:A][TPL:4]Lance: Aaah... [WAI][CLD][TPL:A][TPL:2][LU1:2A][N]I [LU1:FA]run [LU1:D6]time. [FIN]This [LU1:9D]so [N]suddenly, I [LU1:83][LU1:A4][N][LU1:F7]to do... [FIN]I [LU1:82][LU1:F1]to show [N]my face now. [FIN]I'm crying[N][LU1:8E]happiness...[END]`
07BC42 `[TPL:A][TPL:2][LU1:2A][LU1:20][LU2:44]felt[N][LU1:D9]was something[N][LU1:86][LU1:6B]you.[FIN]Now I [LU1:91]I know[N][LU1:F7]the difference is.[FIN]I [LU1:F1]to [LU1:97]you[N]an answer...[END]`
07BCAA `[TPL:A][TPL:2]Lilly:[N]I love you, too.[FIN]I [LU1:F1]to be with[N]you forever...[END]`


;Euro intro sequence
07BE25:
  COP [E0]


07D5E1 #$0120  ;Market line delay


08BA11 `[TPL:9][TPL:4]Walk to the left[N][LU1:F8]a sound!![PAL:0][END]`
08BA7D `[TPL:A][TPL:4]Jackal: [N]I [LU1:A4]the whole [N]story of [LU1:FE]adventure. [FIN]I [LU1:99][LU1:6B]an[N][LU1:6F]bio-technology[N]using a [LU2:55]light. [FIN]I [LU1:83][LU1:A4]it was you.[FIN][LU2:40]the [LU1:C2]to change[N][LU1:72]shape, you could[N]get anything.[FIN][LU1:3D][LU1:F3]bow [N]at [LU1:FE]feet.[FIN][LU1:1D][LU2:89]natural that[N][LU1:27][LU1:11][LU1:F3][N]trick you [LU1:A1]this...[FIN][TPL:1][LU1:25][N]My father!!? [FIN][TPL:4]Jackal: [N]Yes! After all, that's [N]the way kings are. [FIN]He [LU1:F3]do [LU1:6C]to[N]get the power.[FIN]He might even be [LU1:B7][N]evil [LU2:B5]a mercenary [N][LU1:A5]me. Heh heh. [FIN][TPL:1][LU1:25][N]Stop it! [FIN][TPL:4]Jackal: [N]Either way, if I [LU2:84][N]money, [LU1:9F]fine. [FIN][LU1:9][LU1:F0]me to[N][LU1:11]Castle.[PAL:0][END]`
08BC55 `[TPL:A][TPL:0]A [LU1:EE]whispers [N]in Will's head... [FIN]Will... [LU2:27]the [N]Flute....Will....[PAL:0][END]`
08BCF2 `[TPL:9][TPL:4][DLY:0]Jackal: [N]Wa-a-a-a-ah!!!![PAL:0][CLD]`
08BD16 `[TPL:8][TPL:4][DLY:0]Kara...Kara...[PAL:0][CLD]`
08BE66 `[TPL:9][TPL:1][LU1:25][N]Will... [FIN]Why [LU1:B2]everyone[N]hate each other...?[FIN]I...[N]I...[PAL:0][END]`
08BEA9 `[TPL:A][TPL:1][LU1:25][N]I'm sorry... [N]I got upset... [FIN]You are doing [LU1:FE][N][LU2:4D]to [LU1:D1]the world.[FIN]At [LU2:61]I [LU1:A3]wanted[N]to [LU1:90]my father...[FIN]But somehow it got[N]to be a trial.....[FIN]But me.[N]I [LU1:82]regret coming[N]on [LU1:D6]journey...[FIN][LU1:2B]go and [N][LU1:90]the fifth [N]Mystic Statue...[PAL:0][END]`
08BFC7 `[TPL:A]I [LU1:99]a [LU1:EE][LU1:8E]the[N]Flute! [FIN]The [LU1:CD][LU1:EE]I[N][LU1:99]in the prison[N]at [LU1:11]Castle...[FIN][TPL:4][LU2:E][N]Will. [LU1:67]done well to [N][LU1:98][LU1:79][LU1:D6]far. [FIN][TPL:0]Will: [N]Father?! [FIN][TPL:4]Flute:[N]I'm at the Tower now.[FIN]Bring the five Mystic [N]Statues to the Tower. [FIN]The [LU1:D2]you've [N]collected hold the key [N]to the [LU2:6B]of humanity. [FIN]Will..Hurry...The comet[N]is approaching. [FIN][PAL:0][SFX:0]The [LU1:EE]of the Flute[N]quiets and disappears.[PAL:0][END]`


08C561:
  JMP stairway_move_up
  
08C572:
  JMP stairway_move_down


;Remove rest option from save
08DBCE:
  BRA $88DBDE

08DDFE `[CLR]Finished recording...[END]`
08EB68 `[DEF][DLY:9][ADR:EB8F,AAC][N]can now be used![PAU:60][FIN]`
08EB85 `[DEF][CLR][ADR:EBD3,AAC][END]`

0981F4 `[TPL:A][TPL:6][LU1:36][LU2:3E]be [LU1:D9][N]soon, Will.[FIN]Say hello to [N][LU1:FE][LU2:67]for me. [FIN][TPL:0]Will: Thanks. [N]I [LU1:A4]you [LU1:EF][LU2:84][N]a [LU1:95]president. [FIN][TPL:3]Erik: Aaah. [N]I [LU2:6E]I [LU1:FA]see you [N]for a [LU1:A6]time. [FIN][LU1:61]you've finished[N][LU1:FE]business, hurry[N][LU1:73]to [LU1:47]Cape.[FIN][TPL:0]Will: Thank [LU2:C7]I'm [N]glad we all [LU1:B3]the [N]trip together. [FIN][TPL:3]Erik: [N]On [LU1:D6]trip, [LU1:89][N][LU1:8F]something. [FIN]Lance met [LU1:2C]and [N][LU1:8F]his [LU2:7A]father. [FIN][LU2:23]decided to [N][LU1:E7][LU1:BE]his [N]parents' company. [FIN][LU1:26][LU1:CB]to [LU2:95][N]live, and saw a [LU1:F5][N][LU1:BF]the castle. [FIN]I'm [LU1:93]to excuse [N]myself.[FIN]Finally, I can go [N]to the bathroom by [N]myself at night! [FIN][TPL:6][LU1:36][N]Ha ha ha. [LU1:24][LU1:A5]Erik. [FIN][LU1:26]hasn't [LU1:D4][N][LU1:6C]for a while. [FIN]I [LU1:FA]see Will for a [N][LU1:A6]time. [LU1:1F]say [N]goodbye to him. [FIN][TPL:1][LU1:25][N]Hmmm. Right...[FIN][TPL:6][LU1:36][N][LU2:41]reached the [N]Tower of Babel. [FIN]OK, Will. [N]Is [LU1:FE]parachute ready? [N][LU1:2B]go.[PAL:0][END]`
0984B1 `[TPL:A][TPL:0]I jumped out [LU1:BE]the[N]Tower of Babel.[FIN]I hadn't [LU1:70][LU1:D9][N]in a year and a half...[PAL:0][CLD]`

098903 `[TPL:9][TPL:0][SFX:0]One worn-out [LU1:72]is[N]quietly laid down...[FIN]In his head, a familiar [N][LU1:EE]speaks. [FIN][TPL:4][SFX:10]Will. [LU1:1D]me, [N]Olman, [LU1:FE]father. [FIN]My [LU1:72]has decayed, but[N]I [LU1:AF]on [LU1:A5]this...[PAL:0][END]`
0989A2 `[TPL:B][TPL:0]Will: Father![N]Why are you in that[N]form!!![FIN][TPL:4]There's a [LU1:C7]room[N]in the Tower of Babel,[N]filled [LU1:F0]the light[N]of the comet.[FIN]Time goes so fast there[N][LU1:D7][LU1:C1]evolve[N][LU1:ED]quickly...[FIN][TPL:0]Will: [N]Why are [LU1:26]and I [N]able to live?! [FIN][TPL:4]Will's father: [N]Because you two are [N]evolved humans. [FIN][TPL:1][LU1:25][N]Us...? [FIN][TPL:4]Will's father: Long ago [N][LU1:D9]existed biological [N]technology using the [N][LU1:A8]of the comet. [FIN][LU1:3D]freely [LU2:BB]the[N][LU1:C2]to make[N]plants and animals.[FIN]For example, [LU2:AB][LU1:B3][N]the camel. It can go [N]for [LU1:A6]periods [N][LU1:F8]food or water. [FIN][LU1:61][LU1:C1]realized the[N][LU1:C2][LU1:7A]be [LU2:BB]as[N]a weapon,[N][LU1:87][LU1:F2]developed.[FIN]The [LU1:F5]was on the[N]brink of ruin...[FIN]At [LU1:D7]time, the Knights [N]of Darkness and Light [N][LU1:F2]developed to decide [N]the [LU2:6B]of humanity. [FIN]They are [LU1:FE]ancestors.[FIN]The six Mystic Statues[N][LU1:F2][LU1:B3]by the[N]Knights.[FIN]The [LU2:79]Mystic [LU1:48][N]is entrusted to [LU2:C7][END]`
098CAA `[TPL:B][TPL:4][LU2:38]the [LU1:7B][LU1:EF][N]be [LU1:ED]close.[FIN]By then, the two of you [N][LU1:B2]go to the roof [N]of the tower. [N]Close [LU1:FE]eyes....[END]`

098E17 `[DEF][TPL:0][LU1:61]Will and [LU1:26][N]joined and became one [N][LU1:F0]the Light Knight, a [N][LU1:95][LU1:C2]was born... [FIN]The Knights [LU1:F2][LU1:75][N]forth. The [LU1:C]Knight's [N]ultimate power, the [N][LU2:10]was released![PAL:0][END]`
098EB8 `[TPL:D][TPL:4][LU1:68]battle [LU1:EF]change[N]the [LU2:6B]of humanity.[FIN]Now you [LU1:B2]go[N]to the comet!![PAL:0][END]`

099068 `[TPL:F][TPL:4]Will's father: [N]The ancients worshipped [N]the [LU1:7B]as a spirit. [FIN]Those who bathed in [N]the [LU2:55][LU1:A8][LU1:F2][N]given a [LU1:C7]power. [FIN]The [LU1:7B]is [LU2:54]a [N]spirit. But [LU1:9F][N]an unwelcome spirit. [FIN]Evolving too fast[N]brings destruction...[FIN]As [LU1:A6]as people[N][LU1:98]evil hearts,[N][LU1:87][LU1:EF]be born.[FIN]Will, open [LU1:FE][LU2:60][N]and [LU1:A9]around. [END]`
0991A9 `[TPL:F][TPL:4]Will's father: At [LU2:79][N]the [LU1:DF]is near. [FIN]Everyone. [N]Give Will [LU1:FE]power![PAL:0][END]`

099F2A `[TPL:B][TPL:0]Will: [N]Kara!!!? [FIN][TPL:1][LU1:25]I'm sorry. [N]I [LU1:A3][LU1:91]that, if [N]we part now, we'll [N][LU1:BB][LU2:87]again... [FIN][TPL:0]Will: [N]But Kara, why [LU1:98]you [N][LU1:79]here? [FIN]You [LU1:78][LU1:79]here[N]unless you [LU1:98]the[N][LU1:7]Ring...[FIN][TPL:1][LU1:25]Could [LU1:D7][N]be the ring... [N][LU2:7]you [LU1:90]it [N]in the Incan [LU1:19]Ship? [FIN][TPL:0]This [LU1:7]Ring is[N][LU2:5C]blue...[FIN]The ring you [LU1:98]is[N][LU1:A8]blue...[FIN]A [LU1:A8]one[N]and a [LU2:5C]one...[FIN]Will: I understand.. [N]No [LU1:B8][LU1:F7]happens, [N][LU1:82][LU1:AC]me.[PAL:0][END]`


0B81D6 #0F  ;Worm delay 1
0B81DC #0F  ;Worm delay 2
0B8202 #1F  ;Worm delay 3
0B8244 #1B  ;Worm delay 4
0B8282 #13  ;Worm delay 5
0B82AF #11  ;Worm delay 6
0B82D0 #13  ;Worm delay 7
0B82F8 #11  ;Worm delay 8
0B82D0 #13  ;Worm delay 7
0B84EC #1B  ;Worm delay 9

0BD558 `[DLG:3,4][SIZ:D,3][SFX:0][TPL:13][SFX:0][DLY:0][LU1:25][N][LU1:64][LU1:9D]to [N]the comet...? [CLR]That glowing[N]green planet?[CLD]`
0BD5A0 `[DLG:3,4][SIZ:D,3][TPL:15][SFX:0][DLY:0]Will's father: [N]The [LU2:55][LU1:C2][N]has disappeared. [CLR]The evil star has flown[N]off to the [LU1:BD]side[N]of the universe...[CLR][TPL:15][SFX:0]Will. [N]Do you [LU1:A4]what[N]planet [LU1:D7]is, glowing[N][LU1:D9]in the darkness?[CLR][TPL:12][SFX:0]Will: [N]Our Earth...?[CLR][TPL:15][SFX:0]Will's father: [N]That's [LU2:99]Our Earth.[N][CLR]Doesn't it [LU1:A9][LU1:A5]a[N]desert oasis?[CLR][TPL:13][SFX:0][LU1:25][N][LU1:1D][LU1:BB][LU2:A0][N]so beautiful.[CLR]But it [LU1:AA]lonely[N]shining in the dark...[CLD]`
0BD70E `[DLG:3,4][SIZ:D,2][SFX:0][DLY:0][TPL:14][SFX:0][LU1:46]Voice: Yes.[N]The [LU1:F5]is awakened.[CLD]`
0BD740 `[DLG:3,4][SIZ:D,3][SFX:0][DLY:0][CLR][TPL:12][SFX:0]Will: [N]Mother?![CLR][TPL:14][SFX:0]Will's mother:[N]The Earth.[N]A [LU2:85][LU1:F0]millions [N]of children.[CLR]I'm [LU2:A6]you [LU1:DA][N][LU1:6B]us sometimes, [N]and [LU1:26]often [N]thinks [LU1:6B]her parents.[CLR]The [LU1:14]is the same[N]way. She [LU2:6D]lonely if[N]her [LU1:7D]forget[N][LU1:6B]her.[CLR][TPL:15][SFX:0]Will's father: How is it,[N]you two? [N][LU2:1A]at the world[N]you [LU1:AF]in from[N]the outside?[CLR][TPL:13][SFX:0][LU1:25][N][LU1:1D]as if we'd [N][LU1:71]spirits...[CLR][TPL:12][SFX:0]Will: I [LU1:F1]to show [N]all of our group...[CLR]No, I [LU1:F1]to show[N][LU1:89]in the world...[CLR][TPL:15][SFX:0]Will's father: Someday [N][LU1:C1][LU1:EF]build ships [N]to travel the universe.[CLR]Then [LU2:AB][LU1:EF]see[N][LU1:D6]green [LU1:14]with[N][LU1:E1]own eyes.[CLR]See how lonely the[N][LU1:14]looks, [LU1:A3]like[N]the two of you.[CLR]Will's father: Look [N]carefully at [LU1:FE][N]map of the world.[CLR][TPL:12][SFX:0]Will:[N]Ah! The map has started[N]to change![CLD]`
0BD9FA `[DLG:3,4][SIZ:D,3][SFX:0][DLY:0][CLR][TPL:12][SFX:0]Will: [N]Why do you two [N][LU1:A4]the future?[CLR][TPL:15][SFX:0]Will's father: [N][LU1:61]I [LU2:7A]my body, I [N][LU1:CB]seeing everything.[CLR]The past. The future. [N]Humanity's progress.[CLR][LU1:30][LU1:C1]would[N]call [LU1:D6]kind of body[N]a spirit.[CLR][TPL:14][SFX:0]Will's mother:[N]Now you and [LU1:26]can [N][LU1:71]ordinary [N][LU1:7D]again.[CLR][LU1:D]be afraid.[CLR][TPL:13][SFX:0][LU1:25][LU1:61]we [N]return to Earth, [LU1:EF][N]we be separated?[CLR][TPL:15][SFX:0]Will's father: Yes... [N]The [LU1:F5]is changing. [N]Humanity and history, [N][LU1:98][LU1:CB]down a [N]new path.[CLR]You two [LU1:D8][N][LU1:B9]of it [LU1:F6]you [N]met each [LU1:BD]in [N][LU1:47]Cape. [CLR]But [LU1:F6]the Earth[N]needed the Light and[N][LU1:C]Knights, you[N]met again unexpectedly.[CLR][LU1:2B][LU1:A9]at the world[N][LU1:74]the [LU1:C2]of the[N][LU1:7B]is extinguished.[CLR][TPL:14][SFX:0]Will's mother: [N]We [LU2:71]you two [N][LU1:98]a bright future...[CLD]`
0BDC95 `[DLG:3,4][SIZ:D,3][SFX:0][DLY:0][TPL:13][SFX:0][LU1:25]Will... [N][LU1:9]here....[N][LU2:37]me [LU1:FE]face...[CLR]I [LU1:F1]to burn you[N][LU1:A1]my memory.[CLR][LU1:68][LU2:60][LU1:68]nose [N][LU1:68]mouth [LU1:68]hair [N][LU1:68][LU1:EE][N]The warmth of[N][LU1:FE]hand....[CLD]`
0BDD34 `[DLG:3,4][SIZ:D,3][SFX:0][DLY:0][TPL:12][SFX:0]Will: [LU1:D]worry. [N]I [LU1:EF]search you out.[CLR]No [LU1:B8]how long[N]it takes.[N]Hundreds of years...[N]Thousands of years...[N]I [LU1:EF][LU1:79]to you.[CLR]So [LU1:E7]care...[N]Close [LU1:FE]eyes...[CLD]`
0BDDE5 `[DLG:3,4][SIZ:D,3][SFX:0][DLY:0][TPL:13][SFX:0][LU1:25][N]Will....[CLD]`
0BDE02 `[DLG:3,4][SIZ:D,3][SFX:0][DLY:0][TPL:12][SFX:0]Will: [N][LU1:2B]go. [N]To Earth....[CLR][TPL:13][SFX:0][LU1:25][N]Mmmm....[CLD]`
0BDED4 `[DLG:3,13][SIZ:D,3][SFX:0][TPL:0][SFX:0][DLY:0]Will: [N][LU1:49]the land [N]has [LU2:AF]on [N]a [LU1:C7]shape.[CLR][TPL:4][SFX:0]Will's father: [N]That's the new world.[CLR][TPL:1][SFX:0][LU1:25][N]New world?[CLR][TPL:4][SFX:0]Will's father: The path [N]of evolution, [LU1:7F][N]by the comet, has [N]continued [LU2:BC]now.[CLR]The Earth, too,[N]has a life.[N]It, [LU2:B7]has evolved and[N][LU1:7F]its shape.[CLR]Now [LU1:D7]the [LU1:7B]has[N]no influence on the[N]world, [LU1:9F][LU2:96]to[N]its [LU1:C0]condition.[CLD]`

0BE05A #$0000  ;Ending sunset delay

0BE075 `[DEF][SFX:0][DLY:0]The [LU2:B][LU1:A9]had[N]changed, but, glowing[N]in the sky, it was[N]as [LU1:76]as ever.[CLR]Buildings replaced the [N]forests, rivers became [N]roads, but the villages [N]held [LU2:89]smiling faces.[CLR]But the [LU1:14]was[N]the [LU2:89]one[N][LU1:D7][LU2:80]sad.[CLR]Tomorrow morning [N][LU1:26]and I [LU1:EF]start [N]our new lives.[CLR]The Tower of Babel[N][LU1:D5]tall, as if it[N]knows the whole future[N]of the Earth...[CLD]`
