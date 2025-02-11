
------------------------------------------------
?INCLUDE 'scene_meta'
------------------------------------------------
;Fix for spritemap load size (should be a better way to do this)

entry_FB [
  ppu < #00 >   ;00
  music < #1B, #00, @bgm_no_music >   ;01
  bitmap < #00, #10, #10, @gfx_boot_logos, #01 >   ;02
  palette < #00, #50, #80, @pal_boot_logos >   ;03
  spritemap < #$0292, #00, @spm_boot_logos >   ;04
]
