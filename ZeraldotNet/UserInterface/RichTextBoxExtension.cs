using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace UserInterface
{
    public static class RichTextBoxExtension
    {
        public static void Clear(this RichTextBox richTextBox)
        {
            FlowDocument document = new FlowDocument();
            richTextBox.Document = document;
        }
    }
}
