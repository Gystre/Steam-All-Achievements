﻿/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace SAM.Game.Stats
{
    public abstract class StatInfo
    {
        public abstract bool IsModified { get; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public abstract object Value { get; set; }
        public bool IsIncrementOnly { get; set; }
        public int Permission { get; set; }

        public string Extra
        {
            get
            {
                var flags = StatFlags.None;
                flags |= this.IsIncrementOnly == false ? 0 : StatFlags.IncrementOnly;
                flags |= ((this.Permission & 2) != 0) == false ? 0 : StatFlags.Protected;
                flags |= ((this.Permission & ~2) != 0) == false ? 0 : StatFlags.UnknownPermission;
                return flags.ToString();
            }
        }
    }
}
