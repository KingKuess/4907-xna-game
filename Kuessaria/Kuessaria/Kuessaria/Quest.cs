using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    public class Quest
    {
        public enum Status
        {
            notStarted = 0,
            inProgress = 1,
            complete = 2
        }
        public int status;
        public string startText, inProgressText, completeText;
        public int objectiveStatus;
        public int objectiveTarget;
        public int xpReward;

        public Quest(string StartText, string InProgressText, string CompleteText, int target, int reward)
        {
            startText = StartText;
            inProgressText = InProgressText;
            completeText = CompleteText;
            status = 0;
            objectiveTarget = target;
            objectiveStatus = 0;
            xpReward = reward;
        }
        public string getText(int currentStatus)
        {
            string retVal = "Huh? Need something?";
            if (currentStatus == 0)
            {
                retVal = startText;
            }
            else if (currentStatus == 1)
            {
                retVal = inProgressText;
            }
            else if (currentStatus == 2)
            {
                retVal = completeText;
            }
            return retVal;
        }
    }
}
