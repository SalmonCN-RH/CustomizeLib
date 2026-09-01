using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Extra.BulletExtra
{
    public static partial class BulletExtra
    {
        public static BulletHitFilter AnyFilter => anyFilter.Value;

        private static readonly Lazy<BulletHitFilter> anyFilter = new(() =>
        {
            CustomCore.RegisterCustomBulletHitFilter((BulletHitFilter)FilterID.Any, (_) => true);
            return (BulletHitFilter)17500;
        });

        private enum FilterID
        {
            Any = 17500
        }
    }
}
