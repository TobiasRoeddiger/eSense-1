using System;
using System.Collections.Generic;
using System.Text;

namespace Karl.Model
{
	/// <summary>
	/// The Motivation Mode.
	/// </summary>
	class MotivateMode : Mode
	{

		public override void Activate()
		{
			throw new NotImplementedException(); //todo
		}

		public override void Deactivate()
		{
			throw new NotImplementedException(); //todo
		}

		protected override String UpdateName(Lang value)
		{
			return "MotivateMode"; //value.get("mode_motivate");//todo
		}
	}
}
