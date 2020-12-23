/****************************
* 品種管理介面
* 請與品種相關的物件繼承此介面
***************************/

using System.Collections.Generic;

namespace OEP520G.Product
{
    /// <summary>
    /// 品種管理
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>當品種管理員(ProductManager)執行品種切換後，會利用Prism Event Aggregator發佈ProductSwitch事件。</item>
    /// <item>請在ViewModel物件繼承 IProductManager，<br/>
    /// 在建構函式引入Prism IEventAggregator，並訂閱ProductSwitch事件，<br/>
    /// 然後設定事件去呼叫自訂SwProduct函式。</item>
    /// <item>若有需求無法在ViewModel執行切換品種作業時(亦即無法在建構函式引入Prism IEventAggregator)，<br/>
    /// 系統有提供備用的IEventAggregator，但儘量勿用此法(增加物件耦合度)。</item>
    /// <item>詳細說明及範例請參閱原始檔註解。</item>
    /// </list>
    /// </remarks>
    interface IProductManager
    {
        /**************************************************
         1. 當品種管理員(ProductManager)執行品種切換後，會利用 Prism Event Aggregator 發佈事件。
            a.當品種確定切換，系統 Product ID 變更後，便會發佈 SwitchProduct 事件。
            b.當品種切換作業結束後，發佈 AfterSwitchProduct 事件。
        
         2. 請執行以下設計：
              ● 在 ViewModel 物件繼承 IProductManager。
              ● 在 ViewModel 建構函式引入 Prism IEventAggregator。
              ● 設計自訂品種切換函式: SwProduct() 及 AfterSwProduct() 。
              ● 訂閱 SwitchProduct 事件，並設定事件去呼叫 SwProduct() 。
              ● 訂閱 AfterSwitchProduct 事件，並設定事件去呼叫 AfterSwProduct() 。
            範例如下：
        
            using Prism.Events;
            using Prism.Mvvm;
            public class xxxViewModel : BindableBase, IProductManager
            {
                /// <summary>
                /// Event Aggrega
                /// </summary>
                private readonly IEventAggregator _ea;
        
                /// <summary>
                /// 建構函式
                /// </summary>
                public xxxViewModel(IEventAggregator ea)
                {
                    _ea = ea;
        
                    // 訂閱事件，並設定呼叫相對應函式
                    _ea.GetEvent<SwitchProduct>().Subscribe(SwProduct);
                    _ea.GetEvent<AfterSwitchProduct>().Subscribe(AfterSwProduct);
                }
        
                /// <inheritdoc/>
                /// <param name="productId">切換後的品種ID</param>
                public void SwProduct(string productId)
                {
                    // TODO: 品種切換作業
                }
        
                /// <inheritdoc/>
                /// <param name="productId">切換後的品種ID</param>
                public void AfterSwProduct(string productId)
                {
                    // TODO: 品種切換作業
                }
            }
        
         3. 若有建構函式因其他因素無法引入IEventAggregator但有使用IEventAggregator的需求時，
            系統有提供備用的IEventAggregator，但儘量勿用此法(增加物件耦合度)。範例如下：
        
            using OEP520G.Core;
            using Prism.Events;
            public class xxxClass : IProductManager
            {
                /// <summary>
                /// 建構函式
                /// </summary>
                public xxxClass()
                {
                    // 訂閱事件，並設定呼叫相對應函式
                    Common.EA.GetEvent<SwitchProduct>().Subscribe(SwProduct);
                    Common.EA.GetEvent<AfterSwitchProduct>().Subscribe(AfterSwProduct);
                }
        
                /// <inheritdoc/>
                /// <param name="productId">切換後的品種ID</param>
                public void SwProduct(string productId)
                {
                    // TODO: 品種切換作業
                }
        
                /// <inheritdoc/>
                /// <param name="productId">切換後的品種ID</param>
                public void AfterSwProduct(string productId)
                {
                    // TODO: 品種切換作業
                }
            }
        
         4. 關於Prism Event Aggregator完整資料，請參考 https://prismlibrary.com/docs/event-aggregator.html。
        **************************************************/

        //Dictionary<string, bool> RequiresCompleteChangeover { get; set; }

        /// <summary>
        /// 切換品種作業
        /// </summary>
        /// <param name="productId">切換後的品種ID</param>
        void onProductChangeover(string productId);

        /// <summary>
        /// 切換品種作業 (後續作業)
        /// </summary>
        /// <param name="productId">切換後的品種ID</param>
        void afterProductChangeover(string productId);
    }
}
