//=============================================================================
//
// Adventure Game Studio (AGS)
//
// Copyright (C) 1999-2011 Chris Jones and 2011-20xx others
// The full list of copyright holders can be found in the Copyright.txt
// file, which is part of this source code distribution.
//
// The AGS source code is provided under the Artistic License 2.0.
// A copy of this license can be found in the file License.txt and at
// http://www.opensource.org/licenses/artistic-license-2.0.php
//
//=============================================================================

#ifndef __AC_GUITEXTBOX_H
#define __AC_GUITEXTBOX_H

#include <vector>
#include "gui/guiobject.h"
#include "util/string.h"

namespace AGS
{
namespace Common
{

class GUITextBox : public GUIObject
{
public:
    GUITextBox();

    // Operations
    virtual void Draw(Bitmap *ds) override;
 
    // Events
    virtual void OnKeyPress(int keycode) override;
 
    // Serialization
    virtual void WriteToFile(Stream *out) override;
    virtual void ReadFromFile(Stream *in, GuiVersion gui_version) override;
 
// TODO: these members are currently public; hide them later
public:
    int32_t Font;
    String  Text;
    int32_t TextBoxFlags;
    color_t TextColor;

private:
    void DrawTextBoxContents(Bitmap *ds, color_t text_color);
};

} // namespace Common
} // namespace AGS

extern std::vector<AGS::Common::GUITextBox> guitext;
extern int numguitext;

#endif // __AC_GUITEXTBOX_H
