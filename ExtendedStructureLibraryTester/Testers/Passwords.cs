using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSL;

namespace ExtendedStructureLibraryTester.Testers
{
    public class Passwords
    {
        public static readonly string ThreeLetterPass = CSL.Encryption.Passwords.ThreeLetterWordPassword(4);
    }
}
