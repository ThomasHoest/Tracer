using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.VersionCheckUtilities
{
  public interface IFeedbackSender
  {
    bool SendFeedback( string application, string toolID, string from, string subject, string message );
  }
}
