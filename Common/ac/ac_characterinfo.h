#ifndef __AC_CHARACTERINFO_H
#define __AC_CHARACTERINFO_H

#include "ac_defines.h" // constants

#define MAX_INV             301
#define CHF_MANUALSCALING   1
#define CHF_FIXVIEW         2     // between SetCharView and ReleaseCharView
#define CHF_NOINTERACT      4
#define CHF_NODIAGONAL      8
#define CHF_ALWAYSIDLE      0x10
#define CHF_NOLIGHTING      0x20
#define CHF_NOTURNING       0x40
#define CHF_NOWALKBEHINDS   0x80
#define CHF_FLIPSPRITE      0x100  // ?? Is this used??
#define CHF_NOBLOCKING      0x200
#define CHF_SCALEMOVESPEED  0x400
#define CHF_NOBLINKANDTHINK 0x800
#define CHF_SCALEVOLUME     0x1000
#define CHF_HASTINT         0x2000   // engine only
#define CHF_BEHINDSHEPHERD  0x4000   // engine only
#define CHF_AWAITINGMOVE    0x8000   // engine only
#define CHF_MOVENOTWALK     0x10000   // engine only - do not do walk anim
#define CHF_ANTIGLIDE       0x20000
// Speechcol is no longer part of the flags as of v2.5
#define OCHF_SPEECHCOL      0xff000000
#define OCHF_SPEECHCOLSHIFT 24
#define UNIFORM_WALK_SPEED  0
#define FOLLOW_ALWAYSONTOP  0x7ffe
// remember - if change this struct, also change AGSDEFNS.SH and
// plugin header file struct
struct CharacterInfo {
    int   defview;
    int   talkview;
    int   view;
    int   room, prevroom;
    int   x, y, wait;
    int   flags;
    short following;
    short followinfo;
    int   idleview;           // the loop will be randomly picked
    short idletime, idleleft; // num seconds idle before playing anim
    short transparency;       // if character is transparent
    short baseline;
    int   activeinv;
    int   talkcolor;
    int   thinkview;
    short blinkview, blinkinterval; // design time
    short blinktimer, blinkframe;   // run time
    short walkspeed_y, pic_yoffs;
    int   z;    // z-location, for flying etc
    int   walkwait;
    short speech_anim_speed, reserved1;  // only 1 reserved left!!
    short blocking_width, blocking_height;
    int   index_id;  // used for object functions to know the id
    short pic_xoffs, walkwaitcounter;
    short loop, frame;
    short walking, animating;
    short walkspeed, animspeed;
    short inv[MAX_INV];
    short actx, acty;
    char  name[40];
    char  scrname[MAX_SCRIPT_NAME_LEN];
    char  on;

    int get_effective_y();   // return Y - Z
    int get_baseline();      // return baseline, or Y if not set
    int get_blocking_top();    // return Y - BlockingHeight/2
    int get_blocking_bottom(); // return Y + BlockingHeight/2

#ifdef ALLEGRO_BIG_ENDIAN
    void ReadFromFile(FILE *fp);
#endif
};


struct OldCharacterInfo {
    int   defview;
    int   talkview;
    int   view;
    int   room, prevroom;
    int   x, y, wait;
    int   flags;
    short following;
    short followinfo;
    int   idleview;           // the loop will be randomly picked
    short idletime, idleleft; // num seconds idle before playing anim
    short transparency;       // if character is transparent
    short baseline;
    int   activeinv;          // this is an INT to support SeeR (no signed shorts)
    short loop, frame;
    short walking, animating;
    short walkspeed, animspeed;
    short inv[100];
    short actx, acty;
    char  name[30];
    char  scrname[16];
    char  on;
};

#define COPY_CHAR_VAR(name) ci->name = oci->name
void ConvertOldCharacterToNew (OldCharacterInfo *oci, CharacterInfo *ci);

#endif // __AC_CHARACTERINFO_H